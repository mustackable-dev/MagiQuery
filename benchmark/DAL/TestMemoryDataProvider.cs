using System.Data;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebApiExample.DAL.Entities;

namespace Benchmark.DAL;

public class TestMemoryDataProvider: IDataProvider
{
    public IQueryable<Goblin> Goblins { get; private set; } = null!;
    public IQueryable<GoblinCached> CachedGoblins { get; private set; } = null!;
    public void Seed()
    {
        Stream? testDataStream = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream("Benchmark.DAL.Data.TestData.json");

        if (testDataStream is null)
            throw new DataException();
        
        JsonSerializerOptions options = new();
        options.Converters.Add(new JsonStringEnumConverter());

        Goblins = JsonSerializer.Deserialize<Goblin[]>(testDataStream, options)!.AsQueryable();
        
        testDataStream.Position = 0;
        CachedGoblins = JsonSerializer.Deserialize<GoblinCached[]>(testDataStream, options)!.AsQueryable();
    }
}