using System.Data;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using WebApiExample.DAL.Entities;

namespace Benchmark.DAL;

public sealed class TestSqliteDataProvider : DbContext, IDataProvider
{
    public IQueryable<Goblin> Goblins => GoblinsCollection;
    public IQueryable<GoblinCached> CachedGoblins => CachedGoblinsCollection;
    public DbSet<Goblin> GoblinsCollection { get; set; }
    public DbSet<GoblinCached> CachedGoblinsCollection { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<ContractDetails> ContractDetails { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("DataSource=file::memory:?cache=shared");
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
        
        GoblinsCollection.AddRange(goblins);

        testDataStream.Position = 0;
        
        IQueryable<GoblinCached> cachedGoblins = 
            JsonSerializer.Deserialize<GoblinCached[]>(testDataStream, options)!.AsQueryable();
        
        CachedGoblinsCollection.AddRange(cachedGoblins);
        
        SaveChanges();

    }
}