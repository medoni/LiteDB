``` ini

BenchmarkDotNet=v0.11.2, OS=macOS Mojave 10.14 (18A391) [Darwin 18.0.0]
Intel Core i5-3210M CPU 2.50GHz (Ivy Bridge), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=2.1.403
  [Host]     : .NET Core 2.1.5 (CoreCLR 4.6.26919.02, CoreFX 4.6.26919.02), 64bit RyuJIT
  DefaultJob : .NET Core 2.1.5 (CoreCLR 4.6.26919.02, CoreFX 4.6.26919.02), 64bit RyuJIT


```
|    Method | Count |       Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|---------- |------ |-----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
| **Read_File** |   **100** |   **2.685 ms** | **0.0241 ms** | **0.0214 ms** |  **1.00** |    **0.00** |    **492.1875** |      **3.9063** |           **-** |           **761.46 KB** |
| Read_MMap |   100 |   2.595 ms | 0.0281 ms | 0.0249 ms |  0.97 |    0.01 |    492.1875 |      3.9063 |           - |           762.98 KB |
|           |       |            |           |           |       |         |             |             |             |                     |
| **Read_File** | **10000** | **292.973 ms** | **4.2335 ms** | **3.7529 ms** |  **1.00** |    **0.00** |  **35000.0000** |   **1000.0000** |           **-** |         **73383.09 KB** |
| Read_MMap | 10000 | 251.432 ms | 4.9769 ms | 7.6003 ms |  0.84 |    0.02 |  36000.0000 |   1000.0000 |           - |         73376.97 KB |
