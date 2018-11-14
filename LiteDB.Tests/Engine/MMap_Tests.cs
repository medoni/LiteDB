using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;

namespace LiteDB.Tests.Engine
{
    [TestClass]
    public class MMap_Tests
    {
        [TestMethod]
        public void Test_CreateDB() {
            using (var file = new TempFile()) {
                using (var dbA = createEngine(file.Filename)) {
                    dbA.Insert("col", new BsonDocument { { "process", 1 } });
                }

                using (var dbB = createEngine(file.Filename))
                {
                    Assert.AreEqual(1, dbB.Count("col"));
                }
            }
        }


        [TestMethod]
        public void Test_FileLength()
        {
            // test #FileLength Property
            using (var file = new TempFile())
            using (var ds = createDiskService(file.Filename))
            {
                ds.SetLength(10 * BasePage.PAGE_SIZE);
                Assert.AreEqual(10 * BasePage.PAGE_SIZE, ds.FileLength);
            }
        }


        [TestMethod]
        public void Test_ModeMMap()
        {
            using (var file = new TempFile())
            using (var db = createDataBase($"filename={file.Filename};mode=mmap"))
            {
                Assert.AreEqual(typeof(MMapDiskService), db.GetDiskService().GetType());
            }
        }


        [TestMethod]
        public void Test_ModeDefault()
        {
            using (var file = new TempFile())
            using (var db = createDataBase($"filename={file.Filename};"))
            {
                Assert.AreEqual(typeof(MMapDiskService), db.GetDiskService().GetType());
            }

            using (var file = new TempFile())
            using (var ds = createDataBase(file.Filename))
            {
                Assert.AreEqual(typeof(MMapDiskService), ds.GetDiskService().GetType());
            }
        }


        private static MMapDiskService createDiskService(string filename, FileOptions options = null) {
            options = options ?? new FileOptions() { Journal = true };
            var ds = new MMapDiskService(filename, options);
            ds.Initialize(new Logger(), null);
            return ds;
        }

        private static LiteEngine createEngine(string filename, FileOptions options = null) {
            options = options ?? new FileOptions() { Journal = true };
            return new LiteEngine(new MMapDiskService(filename, options));
        }

        private static LiteDatabase createDataBase(string connectionStr) {
            return new LiteDatabase(new ConnectionString(connectionStr));
        }
    }
}
