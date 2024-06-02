```

BenchmarkDotNet v0.13.12, Windows 10 (10.0.19045.4412/22H2/2022Update)
Intel Core i7-8550U CPU 1.80GHz (Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.300
  [Host]   : .NET 8.0.5 (8.0.524.21615), X64 RyuJIT AVX2
  ShortRun : .NET 8.0.5 (8.0.524.21615), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method              | Mean       | Error      | StdDev    | Gen0     | Gen1     | Allocated  |
|-------------------- |-----------:|-----------:|----------:|---------:|---------:|-----------:|
| BenchmarkSmallInput |   1.502 μs |  0.2361 μs | 0.0129 μs |   0.4807 |        - |    1.97 KB |
| BenchmarkLargeInput | 857.634 μs | 96.1658 μs | 5.2712 μs | 185.5469 | 137.6953 | 1010.78 KB |
