using MagiQuery.Models;
using MagiQueryTests.Entities;

namespace MagiQueryTests.Fixtures;

public interface ITestDataFixture: IDisposable
{
    DataProvider Provider { get; }
    IQueryable<Goblin> SampleData { get; }
    IQueryable<Contract> Contracts { get; }
    IQueryable<ContractDetails> ContractDetails { get; }
}