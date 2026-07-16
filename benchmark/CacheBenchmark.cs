using Benchmark.DAL;
using Benchmark.DAL.Utilities;
using BenchmarkDotNet.Attributes;
using Dapper;
using MagiQuery.Extensions;
using MagiQuery.Models;
using Microsoft.Data.Sqlite;
using Ormamu;
using WebApiExample.DAL.Entities;

namespace Benchmark;

[MemoryDiagnoser]
public class CacheBenchmark
{
    private IDataProvider _sqliteProvider = null!;
    private IDataProvider _memoryDataProvider = null!;
    
    private readonly QueryRequestPaged _request = new()
    {
        FilterExpression = "(0 && 1) || (2 && 3)",
        Filters =
        [
            new FilterDefinition
            {
                Property = "Name",
                Operator = FilterOperator.StartsWith,
                Value = "B"
            },
            new FilterDefinition
            {
                Property = "Age",
                Operator = FilterOperator.GreaterThan,
                Value = "30"
            },
            new FilterDefinition
            {
                Property = "DateOfBirth",
                Operator = FilterOperator.LessThanOrEqual,
                Value = "1996-07-11 21:42:09.000"
            },
            new FilterDefinition
            {
                Property = "Taste",
                Operator = FilterOperator.LessThanOrEqual,
                Value = "Bitter"
            }
        ]
    };
    
    [GlobalSetup]
    public void Setup()
    {
        _sqliteProvider = new TestSqliteDataProvider();
        _sqliteProvider.Seed();

        _memoryDataProvider = new TestMemoryDataProvider();
        _memoryDataProvider.Seed();
        
        //Pure Dapper Config
        SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
        
        //Ormamu Config
        OrmamuConfig.Apply(new OrmamuOptions()
        {
            Dialect = SqlDialect.Sqlite
        });
    }

    [Benchmark(Baseline = true)]
    public void QueryOnlyWithoutCacheSqlite() => _sqliteProvider.Goblins.ApplyQuery(_request);
    
    [Benchmark]
    public void QueryOnlyWithCacheSqlite() => _sqliteProvider.CachedGoblins.ApplyQuery(_request);
    
    [Benchmark]
    public void QueryOnlyWithoutCacheMemory() => _memoryDataProvider.Goblins.ApplyQuery(_request);
    
    [Benchmark]
    public void QueryOnlyWithCacheMemory() => _memoryDataProvider.CachedGoblins.ApplyQuery(_request);

    [Benchmark]
    public async Task ResultWithoutCacheMemory()
    {
        await _memoryDataProvider.Goblins.GetPagedResponseAsync(_request);
    }

    [Benchmark]
    public async Task ResultWithCacheMemory()
    {
        await _memoryDataProvider.CachedGoblins.GetPagedResponseAsync(_request);
    }

    [Benchmark]
    public async Task ResultWithoutCacheSqlite()
    {
        await _sqliteProvider.Goblins.GetPagedResponseAsync(_request);
    }

    [Benchmark]
    public async Task ResultWithCacheSqlite()
    {
        await _sqliteProvider.CachedGoblins.GetPagedResponseAsync(_request);
    }

    [Benchmark]
    public async Task DapperResultSqlite()
    {
        await using SqliteConnection connection = new("DataSource=file::memory:?cache=shared");
        _ = (await connection.QueryAsync<Goblin>(
            """
                SELECT "c"."Id", "c"."Age", "c"."Agility", "c"."ContractId", "c"."DateOfBirth", "c"."DateOfConception", "c"."ExperiencePoints", "c"."FavouriteLetter", "c"."HobbitAncestry", "c"."IntelligenceLevel", "c"."IsActive", "c"."MagicPower", "c"."Mana", "c"."Name", "c"."PowerLevel", "c"."Salary", "c"."Stamina", "c"."Strength", "c"."Taste"
                FROM "GoblinsCollection" AS "c"
                WHERE ("c"."Name" LIKE 'B%' AND "c"."Age" > 30) OR ("c"."DateOfBirth" <= '1996-07-11 21:42:09' AND "c"."Taste" <= 3)
                """)).ToList();
    }
    
    [Benchmark]
    public async Task OrmamuResultSqlite()
    {
        await using SqliteConnection connection = new("DataSource=file::memory:?cache=shared");
        _ = (await connection.GetAsync<Goblin>(
                """("Name" LIKE 'B%' AND "Age" > 30) OR ("DateOfBirth" <= '1996-07-11 21:42:09' AND "Taste" <= 3)"""))
            .ToList();
    }
}