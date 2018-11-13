using System;
using BenchmarkDotNet.Running;

namespace LiteDB.Bench
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Engine.FileDiskService_Bench>();
        }
    }
}
