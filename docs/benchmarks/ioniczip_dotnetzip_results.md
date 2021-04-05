# Benchmarking Ionic.Zip and DotNetZip

## Background Information
Porting Assistant/CTA supports automatic migration from Ionic.Zip to DotNetZip through a number of porting rules [[1]("https://github.com/aws/porting-assistant-dotnet-datastore/blob/master/recommendation/ionic.json"), [2](https://github.com/aws/porting-assistant-dotnet-datastore/blob/master/recommendation/ionic.crc.json), [3](https://github.com/aws/porting-assistant-dotnet-datastore/blob/master/recommendation/ionic.zip.json), [4](https://github.com/aws/porting-assistant-dotnet-datastore/blob/master/recommendation/ionic.zlib.json), [5](https://github.com/aws/porting-assistant-dotnet-datastore/blob/master/recommendation/ionic.bzip2.json)]. In order to fully understand the implications of this migration, a benchmark study was done to compare the performance of the two libraries.

## Results Summary
The benchmark study demonstrated that compressing files using DotNetZip on .NET Core does yield a modest performance gain of ~4% over Ionic.Zip on .NET Framework. In most cases, this will be a negligible difference but in extreme cases for large compression jobs, this performance boost may be noticeable.

---

## Benchmark Setup

### Environment 
* OS: Windows 10 Enterprise
* CPU: Intel Xeon 2.4 GHz, 4-cores
* RAM: 16.0 GB
* Benchmark Framework: [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet) v0.12.0

### Subjects

* Ionic.Zip on .NET Framework 4.7.2
* DotNetZip on .NET Core 3.1

### Benchmarking task

* Zip 420 text files
* File size range: 0 KB to 42.5 MB 
* Total of 792 MB

### Execution configuration

* 5 warmup executions
* 5 measured executions

### Execution Statistics

* Ionic.Zip on .NET Framework 4.7.2
    * Mean: 9.679 seconds
    * Error: 0.2577 seconds
    * StdDev: 0.0669 seconds
* DotNetZip on .NET Core 3.1
    * Mean: 9.315 seconds
    * Error: 0.2767 seconds
    * StdDev: 0.0719 seconds
* **DotNetZip targeting netcoreapp3.1 outperformed Ionic.Zip on .NET Framework 4.7.2 by ~4%**

