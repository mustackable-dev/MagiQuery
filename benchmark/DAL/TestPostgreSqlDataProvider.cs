using System.Data;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Benchmark.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Benchmark.DAL;

public sealed class TestPostgreSqlDataProvider: DbContext, IDataProvider
{
    IQueryable<Goblin> IDataProvider.Goblins => Goblins;
    IQueryable<GoblinCached> IDataProvider.CachedGoblins => CachedGoblins;
    public DbSet<Goblin> Goblins { get; set; }
    public DbSet<GoblinCached> CachedGoblins { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<ContractDetails> ContractDetails { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(BenchmarkStaticData.PostgreSqlConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Goblin>()
            .Property(x => x.Taste)
            .HasConversion(
                x=> (int)x,
                y => (Taste)y);
        
        modelBuilder.Entity<GoblinCached>()
            .Property(x => x.Taste)
            .HasConversion(
                x=> (int)x,
                y => (Taste)y);
    }
    
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveColumnType("timestamp without time zone");
        configurationBuilder.Properties<DateTimeOffset>().HaveColumnType("timestamp with time zone");
        base.ConfigureConventions(configurationBuilder);
    }
    public void Seed()
    {
        Database.EnsureDeleted();
        Database.EnsureCreated();
        
        Stream? testDataStream = typeof(IDataProvider)
            .Assembly
            .GetManifestResourceStream("Benchmark.DAL.Data.TestData.json");

        if (testDataStream is null)
            throw new DataException();
        
        JsonSerializerOptions options = new();
        options.Converters.Add(new JsonStringEnumConverter());
        options.Converters.Add(new PostgreSqlDateTimeConverter());
        options.Converters.Add(new PostgreSqlDateTimeOffsetConverter());

        IQueryable<Goblin> rawSampleData = JsonSerializer.Deserialize<Goblin[]>(testDataStream, options)!.AsQueryable();

        testDataStream.Position = 0;
        
        IQueryable<GoblinCached> rawSampleDataCached = JsonSerializer.Deserialize<GoblinCached[]>(testDataStream, options)!.AsQueryable();
        
        foreach (Goblin goblin in rawSampleData)
        {
            goblin.Id = 0;
            if (goblin.Contract is not null)
            {
                goblin.Contract.Id = 0;
                goblin.Contract.Details.Id = 0;
            }

            Goblins.Add(goblin);
            SaveChanges();
        }
        
        foreach (GoblinCached goblin in rawSampleDataCached)
        {
            goblin.Id = 0;
            if (goblin.Contract is not null)
            {
                goblin.Contract.Id = 0;
                goblin.Contract.Details.Id = 0;
            }

            CachedGoblins.Add(goblin);
            SaveChanges();
        }
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