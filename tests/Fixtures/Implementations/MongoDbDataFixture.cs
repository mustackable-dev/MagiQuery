using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using MagiQuery.Models;
using MagiQueryTests.Data;
using MagiQueryTests.Entities;

namespace MagiQueryTests.Fixtures.Implementations;

[CollectionDefinition("MongoDbDataCollection")]
public class MongoDbDataCollection : ICollectionFixture<MongoDbDataFixture>;
public sealed class MongoDbDataFixture: ITestDataFixture
{
    public DataProvider Provider => DataProvider.MongoDb;
    public IQueryable<Goblin> SampleData { get; }
    public IQueryable<Contract> Contracts { get; }
    public IQueryable<ContractDetails> ContractDetails { get; }

    private readonly TestDbContext? _context;

    public MongoDbDataFixture()
    {
        Stream? testDataStream = typeof(ITestDataFixture)
            .Assembly
            .GetManifestResourceStream("MagiQueryTests.Data.TestData.json");
        
        JsonSerializerOptions options = new();
        options.Converters.Add(new JsonStringEnumConverter());
        options.Converters.Add(new MongoDbDateTimeConverter());

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
        
        _context = new(Provider);
        SetUpDatabase();
        SampleData = _context.Goblins;
        Contracts = _context.Contracts;
        ContractDetails = _context.ContractDetails;
    }

    private void SetUpDatabase()
    {
        Goblin[] sampleData = SampleData.OrderBy(x => x.Id).ToArray();
        int mongoDbSubElementsIndex = sampleData.Length;

        foreach (Goblin goblin in sampleData)
        {
            //For Mongo, id needs to be unique
            if (goblin.Contract is not null)
            {
                mongoDbSubElementsIndex++;
                goblin.Contract.Id = mongoDbSubElementsIndex;
                mongoDbSubElementsIndex++;
                goblin.Contract.Details.Id = mongoDbSubElementsIndex;
            }

            _context!.Goblins.Add(goblin);
            _context!.SaveChanges();
        }
    }

    public void Dispose()
    {
        _context?.Dispose();   
        // Method intentionally left empty.
    }
}

public class MongoDbDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.SpecifyKind(DateTime.Parse(reader.GetString()!, CultureInfo.InvariantCulture),
            DateTimeKind.Utc);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(DateTime.SpecifyKind(value, DateTimeKind.Utc));
}