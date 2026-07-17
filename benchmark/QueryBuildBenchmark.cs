using Benchmark.DAL;
using BenchmarkDotNet.Attributes;
using MagiQuery.Extensions;
using MagiQuery.Models;

namespace Benchmark;

[MemoryDiagnoser]
public class QueryBuildBenchmark
{
    private IDataProvider _memoryDataProvider = null!;
    
    [GlobalSetup]
    public void Setup()
    {
        _memoryDataProvider = new TestMemoryDataProvider();
        _memoryDataProvider.Seed();
    }

    [Benchmark(Baseline = true)]
    public void QueryBuild() => _memoryDataProvider.Goblins.ApplyQuery(BenchmarkStaticData.Request);
    
    [Benchmark]
    public void QueryBuildWithCache() => _memoryDataProvider.CachedGoblins.ApplyQuery(BenchmarkStaticData.Request);
}