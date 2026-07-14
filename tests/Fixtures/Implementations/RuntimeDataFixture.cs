using System.Text.Json;
using System.Text.Json.Serialization;
using MagiQuery.Models;
using MagiQueryTests.Entities;

namespace MagiQueryTests.Fixtures.Implementations;

[CollectionDefinition("RuntimeDataCollection")]
public class RuntimeDataCollection : ICollectionFixture<RuntimeDataFixture>;
public sealed class RuntimeDataFixture: ITestDataFixture
{
    public DataProvider Provider => DataProvider.Runtime;
    public IQueryable<Goblin> SampleData { get; }
    public IQueryable<Contract> Contracts { get; }
    public IQueryable<ContractDetails> ContractDetails { get; }

    public RuntimeDataFixture()
    {
        Stream? testDataStream = typeof(ITestDataFixture)
            .Assembly
            .GetManifestResourceStream("MagiQueryTests.Data.TestData.json");
        
        JsonSerializerOptions options = new();
        options.Converters.Add(new JsonStringEnumConverter());

        IQueryable<Goblin> rawSampleData =  testDataStream is not null ? 
            JsonSerializer.Deserialize<Goblin[]>(testDataStream, options)!.AsQueryable():
            (new List<Goblin>()).AsQueryable();
        
        SampleData = rawSampleData;
        Contracts = rawSampleData
                        .Where(x=>x.Contract != null)
                        .Select(x=>x.Contract!)
                        .Distinct()
                        .AsQueryable();
        ContractDetails = rawSampleData
                            .Where(x=>x.Contract != null)
                            .Select(x=>x.Contract!.Details)
                            .Distinct()
                            .AsQueryable();
    }

    public void Dispose()
    {
        // Method intentionally left empty.
    }
}