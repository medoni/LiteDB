using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using LiteDB.Tests;

namespace LiteDB.Bench.Engine_Bench.FileDisk_Bench
{
    [MemoryDiagnoser]
    public class Read_Bench
    {
        [Params(100, 10000)]
        public int Count { get; set; }

        private TempFile _file;
        private TempFile _mmap;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _file = new TempFile();
            using (var db = new LiteEngine(new FileDiskService(_file.Filename)))
            {
                for (var i = 0; i < Count; ++i)
                {
                    db.Insert("col", new BsonDocument { { "_id", i }, { "name", "Lorem" + i } });
                }
            }

            _mmap = new TempFile();
            using (var db = new LiteEngine(new MMapDiskService(_mmap.Filename)))
            {
                for (var i = 0; i < Count; ++i)
                {
                    db.Insert("col", new BsonDocument { { "_id", i }, { "name", "Lorem" + i } });
                }
            }
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _file.Dispose();
            _mmap.Dispose();
        }


        [Benchmark(Baseline = true)]
        public void Read_File()
        {
            using (var db = new LiteEngine(new FileDiskService(_file.Filename)))
            {
                RunTest(db);
            }
        }

        [Benchmark()]
        public void Read_MMap()
        {
            using (var db = new LiteEngine(new MMapDiskService(_mmap.Filename)))
            {
                RunTest(db);
            }
        }

        private void RunTest(LiteEngine db)
        {
            db.EnsureIndex("col", "name");

            for (var i = 0; i < Count; ++i)
            {
                db.Find("col", Query.EQ("name", "Lorem" + i)).ToArray();
                //db.GetCacheService().ClearPages();
            }
        }
    }
}
