``` ini

BenchmarkDotNet=v0.11.2, OS=macOS Mojave 10.14 (18A391) [Darwin 18.0.0]
Intel Core i5-3210M CPU 2.50GHz (Ivy Bridge), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=2.1.403
  [Host]     : .NET Core 2.1.5 (CoreCLR 4.6.26919.02, CoreFX 4.6.26919.02), 64bit RyuJIT
  DefaultJob : .NET Core 2.1.5 (CoreCLR 4.6.26919.02, CoreFX 4.6.26919.02), 64bit RyuJIT


```
|         Method |     Mean |    Error |   StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|--------------- |---------:|---------:|---------:|------:|--------:|------------:|------------:|------------:|--------------------:|
| Insert100_File | 409.4 ms | 5.939 ms | 5.265 ms |  1.00 |    0.00 |  20000.0000 |           - |           - |            30.65 MB |
| Insert100_MMap | 358.1 ms | 7.022 ms | 7.211 ms |  0.87 |    0.02 |  18000.0000 |           - |           - |            27.33 MB |
