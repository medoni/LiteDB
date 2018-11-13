``` ini

BenchmarkDotNet=v0.11.2, OS=macOS Mojave 10.14 (18A391) [Darwin 18.0.0]
Intel Core i5-3210M CPU 2.50GHz (Ivy Bridge), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=2.1.403
  [Host]     : .NET Core 2.1.5 (CoreCLR 4.6.26919.02, CoreFX 4.6.26919.02), 64bit RyuJIT
  DefaultJob : .NET Core 2.1.5 (CoreCLR 4.6.26919.02, CoreFX 4.6.26919.02), 64bit RyuJIT


```
|         Method |      Mean |    Error |   StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|--------------- |----------:|---------:|---------:|------:|--------:|------------:|------------:|------------:|--------------------:|
| Insert100_File |  68.89 ms | 4.577 ms | 12.99 ms |  1.00 |    0.00 |   4000.0000 |           - |           - |             7.26 MB |
| Insert100_MMap | 314.47 ms | 6.203 ms | 11.34 ms |  4.72 |    0.44 |   5000.0000 |           - |           - |             7.82 MB |
