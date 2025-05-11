using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using MagiQueryTests.Data;
using MagiQueryTests.Entities;
using MagiQuery.Models;

namespace MagiQueryTests.Fixtures;
public sealed class TestDataFixture: IDisposable
{
    public static readonly DataProvider Provider = DataProvider.Runtime;
    public IQueryable<Goblin> SampleData { get; }
    public IQueryable<Contract> Contracts { get; }
    public IQueryable<ContractDetails> ContractDetails { get; }

    private readonly TestDbContext? _context;

    public TestDataFixture()
    {
        Stream? testDataStream = typeof(TestDataFixture)
            .Assembly
            .GetManifestResourceStream("MagiQueryTests.Data.TestData.json");
        
        JsonSerializerOptions options = new();
        options.Converters.Add(new JsonStringEnumConverter());

        switch (Provider)
        {
            case DataProvider.PostgreSql:
                options.Converters.Add(new PostgreSqlDateTimeConverter());
                options.Converters.Add(new PostgreSqlDateTimeOffsetConverter());
                break;
            case DataProvider.MongoDb:
                options.Converters.Add(new MongoDbDateTimeConverter());
                break;
        }

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
        
        if (Provider is not DataProvider.Runtime)
        {
            _context = new(Provider);
            SetUpDatabase();
            SampleData = _context.Goblins;
            Contracts = _context.Contracts;
            ContractDetails = _context.ContractDetails;
        }
    }

    private void SetUpDatabase()
    {
        Goblin[] sampleData = SampleData.OrderBy(x => x.Id).ToArray();
        int mongoDbSubElementsIndex = sampleData.Length;

        foreach (Goblin goblin in sampleData)
        {
            if (Provider != DataProvider.MongoDb)
            {
                //Will get an autoincrement error if we try to force Id values for SQL databases
                goblin.Id = 0;
                if (goblin.Contract is not null)
                {
                    goblin.Contract.Id = 0;
                    goblin.Contract.Details.Id = 0;
                }
            }
            else
            {
                //For Mongo, id needs to be unique
                if (goblin.Contract is not null)
                {
                    mongoDbSubElementsIndex++;
                    goblin.Contract.Id = mongoDbSubElementsIndex;
                    mongoDbSubElementsIndex++;
                    goblin.Contract.Details.Id = mongoDbSubElementsIndex;
                }
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
        //Setting timezone to unspecified to avoid issues with timestamp without timezone column in PostgreSQL
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

public class MongoDbDateTimeConverter: JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.SpecifyKind(DateTime.Parse(reader.GetString()!, CultureInfo.InvariantCulture),DateTimeKind.Utc);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        //Setting timezone to unspecified to avoid issues with timestamp without timezone column in PostgreSQL
        writer.WriteStringValue(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }
}