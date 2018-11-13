using System;
using BenchmarkDotNet.Attributes;
using LiteDB.Tests;

namespace LiteDB.Bench.Engine
{
    [MemoryDiagnoser]
    public class FileDiskService_Bench
    {

        [Benchmark(Baseline = true)]
        public void Insert100_File() {
            using (var file = new TempFile())
            using (var db = new LiteEngine(new FileDiskService(file.Filename)))
            {
                for (var i = 0; i < 100; ++i) {
                    db.Insert("col", new BsonDocument { { "_id", i }, { "name", "Lorem" + i } });
                }
            }
        }

        [Benchmark()]
        public void Insert100_MMap()
        {
            using (var file = new TempFile())
            using (var db = new LiteEngine(new MMapDiskService(file.Filename)))
            {
                for (var i = 0; i < 100; ++i)
                {
                    db.Insert("col", new BsonDocument { { "_id", i }, { "name", "Lorem" + i } });
                }
            }
        }

    }
}
