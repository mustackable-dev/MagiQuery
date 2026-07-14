using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using MagiQuery.Models;
using MagiQueryTests.Data;
using MagiQueryTests.Entities;

namespace MagiQueryTests.Fixtures.Implementations;

[CollectionDefinition("PostgreSqlDataCollection")]
public class PostgreSqlDataCollection : ICollectionFixture<PostgreSqlDataFixture>;
public sealed class PostgreSqlDataFixture: ITestDataFixture
{
    public DataProvider Provider => DataProvider.PostgreSql;
    public IQueryable<Goblin> SampleData { get; }
    public IQueryable<Contract> Contracts { get; }
    public IQueryable<ContractDetails> ContractDetails { get; }

    private readonly TestDbContext? _context;

    public PostgreSqlDataFixture()
    {
        Stream? testDataStream = typeof(ITestDataFixture)
            .Assembly
            .GetManifestResourceStream("MagiQueryTests.Data.TestData.json");
        
        JsonSerializerOptions options = new();
        options.Converters.Add(new JsonStringEnumConverter());
        options.Converters.Add(new PostgreSqlDateTimeConverter());
        options.Converters.Add(new PostgreSqlDateTimeOffsetConverter());

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
        
        _context = new(DataProvider.PostgreSql);
        SetUpDatabase();
        SampleData = _context.Goblins;
        Contracts = _context.Contracts;
        ContractDetails = _context.ContractDetails;
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

    public void Dispose()
    {
        _context?.Dispose();   
        // Method intentionally left empty.
    }
}

public class PostgreSqlDateTimeConverter: JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.SpecifyKind(DateTime.Parse(reader.GetString()!, CultureInfo.InvariantCulture),DateTimeKind.Unspecified);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        //Setting timezone to DateTimeKind.Unspecified to avoid issues with timestamp without timezone column in PostgreSQL
        writer.WriteStringValue(DateTime.SpecifyKind(value, DateTimeKind.Unspecified));
    }
}

public class PostgreSqlDateTimeOffsetConverter: JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTimeOffset.Parse(reader.GetString()!, CultureInfo.InvariantCulture).ToUniversalTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}