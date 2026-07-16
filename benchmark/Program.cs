using Benchmark;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<CacheBenchmark>();
// var benchmark = new CacheBenchmark();
//
// benchmark.Setup();
// await benchmark.OrmamuResultSqlite();