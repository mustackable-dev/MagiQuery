using System.Text.Json;
using System.Text.Json.Serialization;
using MagiQuery.Models;
using MagiQueryTests.Data;
using MagiQueryTests.Entities;

namespace MagiQueryTests.Fixtures.Implementations;

[CollectionDefinition("SqlServerDataCollection")]
public class SqlServerDataCollection : ICollectionFixture<SqlServerDataFixture>;
public sealed class SqlServerDataFixture: ITestDataFixture
{
    public DataProvider Provider => DataProvider.SqlServer;
    public IQueryable<Goblin> SampleData { get; }
    public IQueryable<Contract> Contracts { get; }
    public IQueryable<ContractDetails> ContractDetails { get; }

    private readonly TestDbContext? _context;

    public SqlServerDataFixture()
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
        
        
        _context = new(DataProvider.SqlServer);
        SetUpDatabase();
        SampleData = _context.Goblins;
        Contracts = _context.Contracts;
        ContractDetails = _context.ContractDetails;
    }

    public void Dispose()
    {
        _context?.Dispose();   
    }

    private void SetUpDatabase()
    {
        Goblin[] sampleData = SampleData.OrderBy(x => x.Id).ToArray();

        foreach (Goblin goblin in sampleData)
        {
            //Will get an autoincrement error if we try to force Id values for SQL databases
            goblin.Id = 0;
            if (goblin.Contract is not null)
            {
                goblin.Contract.Id = 0;
                goblin.Contract.Details.Id = 0;
            }

            _context!.Goblins.Add(goblin);
            _context!.SaveChanges();
        }
    }
}