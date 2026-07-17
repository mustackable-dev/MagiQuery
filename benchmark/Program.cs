using Benchmark;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<QueryBuildBenchmark>();
BenchmarkRunner.Run<ResultFetchBenchmark>();