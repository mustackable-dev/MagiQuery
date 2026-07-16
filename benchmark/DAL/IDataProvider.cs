using WebApiExample.DAL.Entities;

namespace Benchmark.DAL;

public interface IDataProvider
{
    IQueryable<Goblin> Goblins { get; }
    IQueryable<GoblinCached> CachedGoblins { get; }
    void Seed();
}