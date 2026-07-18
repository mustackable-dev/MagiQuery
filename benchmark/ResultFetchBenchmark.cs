using Benchmark.DAL;
using Benchmark.DAL.Entities;
using BenchmarkDotNet.Attributes;
using Dapper;
using MagiQuery.Extensions;
using MagiQuery.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Benchmark;

[MemoryDiagnoser]
public class ResultFetchBenchmark
{
    private IDataProvider _sqlDataProvider = null!;
    
    [Params(DataProvider.Sqlite, DataProvider.PostgreSql)]
    public DataProvider DatabaseType { get; set; }
    
    [GlobalSetup]
    public void Setup()
    {
        _sqlDataProvider = DatabaseType switch
        {
            DataProvider.Sqlite => new TestSqliteDataProvider(),
            DataProvider.PostgreSql => new TestPostgreSqlDataProvider(),
            _ => throw new KeyNotFoundException("Missing SQL data provider")
        };
        
        _sqlDataProvider.Seed();
    }
    
    [Benchmark(Baseline = true)]
    public async Task EFCore()
    {
        await 
            _sqlDataProvider.Goblins.Where(x=>
                    (x.Name.StartsWith("B") && x.Age > 30) ||
                    (x.DateOfBirth<=new DateTime(1996, 7, 11, 21, 42, 9) && x.Taste!=Taste.Bitter))
                .AsNoTracking()
                .ToListAsync();
    }

    [Benchmark]
    public async Task MagiQuery()
    {
        await _sqlDataProvider.Goblins.ApplyQuery(BenchmarkStaticData.Request).AsNoTracking().ToListAsync();
    }

    [Benchmark]
    public async Task Dapper()
    {
        switch (DatabaseType)
        {
            case DataProvider.Sqlite:
            {
                await using SqliteConnection connection = new(BenchmarkStaticData.SqliteConnectionString);
                
                _ = (await connection.QueryAsync<Goblin>(
                    """
                    SELECT "c"."Id", "c"."Age", "c"."Agility", "c"."ContractId", "c"."DateOfBirth", "c"."ExperiencePoints", "c"."FavouriteLetter", "c"."HobbitAncestry", "c"."IntelligenceLevel", "c"."IsActive", "c"."MagicPower", "c"."Mana", "c"."Name", "c"."PowerLevel", "c"."Salary", "c"."Stamina", "c"."Strength", "c"."Taste"
                    FROM "Goblins" AS "c"
                    WHERE ("c"."Name" LIKE 'B%' AND "c"."Age" > 30) OR ("c"."DateOfBirth" <= '1996-07-11 21:42:09' AND "c"."Taste" <> 3)
                    """)).ToList();
                
                break;
            }
            case DataProvider.PostgreSql:
            {
                await using NpgsqlConnection connection = new(BenchmarkStaticData.PostgreSqlConnectionString);
                
                _ = (await connection.QueryAsync<Goblin>(
                    """
                    SELECT g."Id", g."Age", g."Agility", g."ContractId", g."DateOfBirth", g."ExperiencePoints", g."FavouriteLetter", g."HobbitAncestry", g."IntelligenceLevel", g."IsActive", g."MagicPower", g."Mana", g."Name", g."PowerLevel", g."Salary", g."Stamina", g."Strength", g."Taste"
                    FROM "Goblins" AS g
                    WHERE (g."Name" LIKE 'B%' AND g."Age" > 30) OR (g."DateOfBirth" <= TIMESTAMP '1996-07-11T21:42:09' AND g."Taste" <> 3)
                    """)).ToList();
                
                break;
            }
            default:
                throw new KeyNotFoundException("Missing SQL data provider");
        }
    }
}