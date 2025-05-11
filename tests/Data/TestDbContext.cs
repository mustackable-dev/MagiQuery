using MagiQueryTests.Entities;
using MagiQuery.Models;
using Microsoft.EntityFrameworkCore;

namespace MagiQueryTests.Data;
public sealed class TestDbContext : DbContext
{
    private readonly string _connectionString;
    private readonly DataProvider _variant;
    public DbSet<Goblin> Goblins { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<ContractDetails> ContractDetails { get; set; }

    public TestDbContext(DataProvider variant)
    {
        _variant = variant;
        _connectionString = variant switch
        {
            DataProvider.Sqlite => 
                "DataSource=file::memory:?cache=shared",
            DataProvider.SqlServer => 
                "Data Source=localhost,1434;TrustServerCertificate=true;Initial Catalog=MagiQueryTests;" +
                "User ID=sa;Password=Test123!",
            DataProvider.PostgreSql =>
                "User ID=postgres;Password=Test123!;Host=localhost;Port=5433;Database=MagiQueryTests;" +
                "Pooling=true;Connection Lifetime=0;",
            DataProvider.MySql =>
                "Server=localhost;Port=3307;Database=MagiQueryTests;Uid=root;Pwd=Test123!;",
            DataProvider.MySqlPomelo =>
                "Server=localhost;Port=3305;Database=MagiQueryTests;Uid=root;Pwd=Test123!;SslMode=DISABLED;",
            DataProvider.MongoDb =>
                "mongodb://localhost:27016",
            _=> ""
        };

        if (variant == DataProvider.MongoDb) Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
        
        Database.EnsureDeleted();
        Database.EnsureCreated();

    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        if (_variant == DataProvider.PostgreSql)
        {
            configurationBuilder.Properties<DateTime>().HaveColumnType("timestamp without time zone");
            configurationBuilder.Properties<DateTimeOffset>().HaveColumnType("timestamp with time zone");
        }
        base.ConfigureConventions(configurationBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        switch (_variant)
        {
            case DataProvider.InMemory:
                optionsBuilder.UseInMemoryDatabase("Test");
                break;
            case DataProvider.Sqlite:
                optionsBuilder.UseSqlite(_connectionString);
                break;
            case DataProvider.SqlServer:
                optionsBuilder.UseSqlServer(_connectionString);
                optionsBuilder.LogTo(Console.WriteLine);
                break;
            case DataProvider.PostgreSql:
                optionsBuilder.UseNpgsql(_connectionString);
                break;
            case DataProvider.MySql:
                optionsBuilder.UseMySQL(_connectionString);
                break;
            case DataProvider.MySqlPomelo:
                optionsBuilder.UseMySql(_connectionString, ServerVersion.AutoDetect(_connectionString));
                break;
            case DataProvider.MongoDb:
                optionsBuilder.UseMongoDB(_connectionString, "MagiQueryTests");
                break;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        switch (_variant)
        {
            case DataProvider.Sqlite:
                modelBuilder.Entity<Goblin>()
                    .Property(x => x.Taste)
                    .HasConversion(
                        x=> (int)x,
                        y => (Taste)y);
                break;
        }
    }
}