using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using WebApiExample.DAL.Entities;

namespace WebApiExample.DAL;
public sealed class TestDbContext : DbContext
{
    public DbSet<Goblin> Goblins { get; set; }
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
    }

    public void Seed()
    {
        Database.EnsureCreated();
        
        Stream? testDataStream = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream("WebApiExample.DAL.Data.TestData.json");
        
        JsonSerializerOptions options = new();
        options.Converters.Add(new JsonStringEnumConverter());
        
        IQueryable<Goblin> rawSampleData =  testDataStream is not null ? 
            JsonSerializer.Deserialize<Goblin[]>(testDataStream, options)!.AsQueryable():
            new List<Goblin>().AsQueryable();
        
        Goblins.AddRange(rawSampleData);
        SaveChanges();

        ThreadPool.QueueUserWorkItem(_ =>
        {
            KeepSqliteDatabaseAlive(CancellationToken.None).Wait();
        });

    }

    // SQLite in memory databases are disposed after a certain period if there are no active connections to them,
    // so we are starting a little worker to keep the database alive.
    private async Task KeepSqliteDatabaseAlive(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _ = await Goblins.FirstOrDefaultAsync(cancellationToken); 
            await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
        }
    }
}