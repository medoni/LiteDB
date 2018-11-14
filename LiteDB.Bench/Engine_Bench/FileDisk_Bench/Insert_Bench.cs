using System;
using BenchmarkDotNet.Attributes;
using LiteDB.Tests;

namespace LiteDB.Bench.Engine_Bench.FileDisk_Bench
{
    [MemoryDiagnoser]
    public class Insert_Bench
    {
        [Params(100, 10000)]
        public int Count { get; set; }


        [Benchmark(Baseline = true)]
        public void Insert_File() {
            using (var file = new TempFile())
            using (var db = new LiteEngine(new FileDiskService(file.Filename)))
            {
                for (var i = 0; i < Count; ++i) {
                    db.Insert("col", new BsonDocument { { "_id", i }, { "name", "Lorem" + i } });
                }
            }
        }

        [Benchmark()]
        public void Insert_MMap()
        {
            using (var file = new TempFile())
            using (var db = new LiteEngine(new MMapDiskService(file.Filename)))
            {
                for (var i = 0; i < Count; ++i)
                {
                    db.Insert("col", new BsonDocument { { "_id", i }, { "name", "Lorem" + i } });
                }
            }
        }


    }
}
