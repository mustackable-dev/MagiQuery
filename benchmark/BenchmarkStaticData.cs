using MagiQuery.Models;

namespace Benchmark;

public static class BenchmarkStaticData
{
    public static QueryRequestPaged Request { get; } = new()
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
                Operator = FilterOperator.DoesNotEqual,
                Value = "Bitter"
            }
        ]
    };
    
    public const string PostgreSqlConnectionString =
        "User ID=postgres;Password=Test123!;Host=localhost;Port=5433;Database=MagiQueryTests;" +
        "Pooling=true;Connection Lifetime=0;";
    
    public const string SqliteConnectionString = "DataSource=file::memory:?cache=shared";
}