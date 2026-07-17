using System.Data;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Benchmark.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Benchmark.DAL;

public sealed class TestSqliteDataProvider : DbContext, IDataProvider
{
    IQueryable<Goblin> IDataProvider.Goblins => Goblins;
    IQueryable<GoblinCached> IDataProvider.CachedGoblins => CachedGoblins;
    public DbSet<Goblin> Goblins { get; set; }
    public DbSet<GoblinCached> CachedGoblins { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<ContractDetails> ContractDetails { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(BenchmarkStaticData.SqliteConnectionString);
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

    public void Seed()
    {
        Database.EnsureCreated();
        
        Stream? testDataStream = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream("Benchmark.DAL.Data.TestData.json");

        if (testDataStream is null)
            throw new DataException();
        
        JsonSerializerOptions options = new();
        options.Converters.Add(new JsonStringEnumConverter());

        IQueryable<Goblin> goblins = 
            JsonSerializer.Deserialize<Goblin[]>(testDataStream, options)!.AsQueryable();
        
        Goblins.AddRange(goblins);

        testDataStream.Position = 0;
        
        IQueryable<GoblinCached> cachedGoblins = 
            JsonSerializer.Deserialize<GoblinCached[]>(testDataStream, options)!.AsQueryable();
        
        CachedGoblins.AddRange(cachedGoblins);
        
        SaveChanges();

    }
}