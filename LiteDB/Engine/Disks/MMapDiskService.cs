using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading;

namespace LiteDB
{
    public class MMapDiskService : IDiskService
    {
        /// <summary>
        /// Position, on page, about page type
        /// </summary>
        internal const int PAGE_TYPE_POSITION = FileDiskService.PAGE_TYPE_POSITION;
        internal const int CREATE_CAPACITY = BasePage.PAGE_SIZE * 4;

        internal const int TIMEOUT_CREATEMMAP = 1000;
        internal const int TIMEOUT_CREATEFILESTREAM = 10000;

        private readonly string _filename;

        private Logger _log; // will be initialize in "Initialize()"
        private FileOptions _options;

        private Random _lockReadRand = new Random();

        #region Initialize/Dispose disk

        public MMapDiskService(string filename, bool journal = true)
            : this(filename, new FileOptions { Journal = journal })
        {
        }

        public MMapDiskService(string filename, FileOptions options)
        {
            // simple validations
            if (filename.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(filename));
            if (options.InitialSize > options.LimitSize) throw new ArgumentException("limit size less than initial size");

            // setting class variables
            _filename = filename;
            _options = options;
        }

        public void Initialize(Logger log, string password)
        {
            if (_options.FileMode != FileMode.MMap) {
                throw LiteException.MMapFileModeNotSupported(_options.FileMode);
            }

            // get log instance to disk
            _log = log;

            // if is read only, journal must be disabled
            if (_options.FileMode == FileMode.ReadOnly) _options.Journal = false;

            _log.Write(Logger.DISK, "open datafile '{0}'", Path.GetFileName(_filename));

            long fileSize = File.Exists(_filename) ? new FileInfo(_filename).Length : 0;
            long capacity = Math.Max(CREATE_CAPACITY, _options.InitialSize);
            capacity = Math.Max(capacity, fileSize);
            CreateMMap(capacity);

            // if file is new, initialize
            if (fileSize == 0)
            {
                CreateNewDB(password);
            }
        }

        public virtual void Dispose()
        {
            if (_mmap != null)
            {
                _log.Write(Logger.DISK, "close datafile '{0}'", Path.GetFileName(_filename));
                _mmap.Dispose();
                _mmapStream.Dispose();
                _mmap = null;
                _mmapStream = null;
            }
        }

        #endregion

        #region createNew

        private void CreateNewDB(string password)
        {
            _log.Write(Logger.DISK, "initialize new datafile");

            LiteEngine.CreateDatabase(_mmapStream, password, _options.InitialSize);
        }

        #endregion

        #region File Access

        private MemoryMappedFile _mmap;
        private MemoryMappedViewStream _mmapStream;
        private long _capacity;

        private void CreateMMap(long capacity) {
            if (_mmap != null) {
                _mmapStream?.Dispose();
                _mmap.Dispose();
            }

            SpinWait.SpinUntil(() =>
            {
                try
                {
                    _mmap = MemoryMappedFile.CreateFromFile(_filename,
                        _options.FileMode == FileMode.ReadOnly ? System.IO.FileMode.Open : System.IO.FileMode.OpenOrCreate,
                        null, capacity
                    );
                    return true;
                } catch (IOException)
                {
                    return false;
                }
            }, TIMEOUT_CREATEMMAP);

            
            _capacity = capacity;

            _mmapStream = _mmap.CreateViewStream(0, capacity, MemoryMappedFileAccess.ReadWrite); 
        }


        private void EnsureSize(long capacity, bool shrink = false) {
            if (capacity > _capacity) {
                CreateMMap(capacity);
            }
        }

        private Stream CreateFileStream(int timeout, System.IO.FileMode fileMode, FileAccess fileAccess, FileShare fileShare) {
            Stream outStream = null;

            outStream = new FileStream(_filename, fileMode, fileAccess, fileShare);

            return outStream;
        }

        private void ShrinkFile(long capacity) {
            if (_mmap != null)
            {
                _mmap.Dispose();
                _mmapStream.Dispose();
            }

            using (var fs = CreateFileStream(TIMEOUT_CREATEFILESTREAM, System.IO.FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                fs.SetLength(capacity);
            }
            CreateMMap(capacity);
        }

        #endregion

        #region Read/Write

        /// <summary>
        /// Read page bytes from disk
        /// </summary>
        public virtual byte[] ReadPage(uint pageID)
        {
            var buffer = new byte[BasePage.PAGE_SIZE];
            var position = BasePage.GetSizeOfPages(pageID);

            EnsureSize(position + BasePage.PAGE_SIZE);

            _mmapStream.Seek(position, SeekOrigin.Begin);
            _mmapStream.Read(buffer, 0, BasePage.PAGE_SIZE);

            _log.Write(Logger.DISK, "read page #{0:0000} :: {1}", pageID, (PageType)buffer[PAGE_TYPE_POSITION]);

            return buffer;
        }

        /// <summary>
        /// Persist single page bytes to disk
        /// </summary>
        public virtual void WritePage(uint pageID, byte[] buffer)
        {
            var position = BasePage.GetSizeOfPages(pageID);

            _log.Write(Logger.DISK, "write page #{0:0000} :: {1}", pageID, (PageType)buffer[PAGE_TYPE_POSITION]);

            EnsureSize(position + BasePage.PAGE_SIZE);

            _mmapStream.Seek(position, SeekOrigin.Begin);
            _mmapStream.Write(buffer, 0, BasePage.PAGE_SIZE);
        }

        /// <summary>
        /// Set datafile length
        /// </summary>
        public void SetLength(long fileSize)
        {
            EnsureSize(fileSize, shrink: true);
        }

        /// <summary>
        /// Returns file length
        /// </summary>
        public long FileLength { get { return new FileInfo(_filename).Length; } }

        #endregion

        #region Journal file

        /// <summary>
        /// Indicate if journal are enabled or not based on file options
        /// </summary>
        public bool IsJournalEnabled { get { return _options.Journal; } }

        /// <summary>
        /// Write original bytes page in a journal file (in sequence) - if journal not exists, create.
        /// </summary>
        public void WriteJournal(ICollection<byte[]> pages, uint lastPageID)
        {
            // write journal only if enabled
            if (_options.Journal == false) return;

            var size = BasePage.GetSizeOfPages(lastPageID + 1) +
                BasePage.GetSizeOfPages(pages.Count);

            _log.Write(Logger.JOURNAL, "extend datafile to journal - {0} pages", pages.Count);

            var startPosition = BasePage.GetSizeOfPages(lastPageID + 1);
            var requiredSize = BasePage.GetSizeOfPages(pages.Count);

            EnsureSize(startPosition + requiredSize);
            _mmapStream.Seek(startPosition, SeekOrigin.Begin);

            foreach (var buffer in pages)
            {
                // read pageID and pageType from buffer
                var pageID = BitConverter.ToUInt32(buffer, 0);
                var pageType = (PageType)buffer[PAGE_TYPE_POSITION];

                _log.Write(Logger.JOURNAL, "write page #{0:0000} :: {1}", pageID, pageType);

                // write page bytes
                _mmapStream.Write(buffer, 0, BasePage.PAGE_SIZE);
            }
        }

        /// <summary>
        /// Read journal file returning IEnumerable of pages
        /// </summary>
        public IEnumerable<byte[]> ReadJournal(uint lastPageID)
        {
            var startPos = BasePage.GetSizeOfPages(lastPageID + 1);
            var requiredSize = FileLength - startPos;

            var buffer = new byte[BasePage.PAGE_SIZE];
            var pos = startPos;
            var endPos = startPos + requiredSize;

            EnsureSize(endPos);
            _mmapStream.Seek(startPos, SeekOrigin.Begin);

            while (pos < endPos)
            {
                // read page bytes from journal file
                _mmapStream.Read(buffer, 0, BasePage.PAGE_SIZE);
                yield return buffer;
            }
        }

        /// <summary>
        /// Shrink datafile to crop journal area
        /// </summary>
        public void ClearJournal(uint lastPageID)
        {
            _log.Write(Logger.JOURNAL, "shrink datafile to remove journal area");

            this.SetLength(BasePage.GetSizeOfPages(lastPageID + 1));
        }

        /// <summary>
        /// Flush data from memory to disk
        /// </summary>
        public void Flush()
        {
        }

        #endregion

        #region Lock / Unlock

        /// <summary>
        /// Indicate disk can be access by multiples processes or not
        /// </summary>
        public bool IsExclusive { get { return false; } }

        /// <summary>
        /// Implement datafile lock. Return lock position
        /// </summary>
        public int Lock(LockState state, TimeSpan timeout)
        {
            // METODO: necessary when using MemoryMappedFileAccess?
            return 0;
        }

        /// <summary>
        /// Unlock datafile based on state and position
        /// </summary>
        public void Unlock(LockState state, int position)
        {
        }

        #endregion
    }
}
