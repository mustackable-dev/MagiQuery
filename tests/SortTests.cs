using MagiQueryTests.Fixtures;
using MagiQueryTests.Fixtures.Implementations;
using MagiQueryTests.BaseTests;

namespace Sort;

[Collection("RuntimeDataCollection")]
public class Runtime(RuntimeDataFixture fixture) : SortTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("InMemoryDataCollection")]
public class InMemory(InMemoryDataFixture fixture) : SortTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("SqliteDataCollection")]
public class Sqlite(SqliteDataFixture fixture) : SortTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("SqlServerDataCollection")]
public class SqlServer(SqlServerDataFixture fixture) : SortTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("PostgreSqlDataCollection")]
public class PostgreSql(PostgreSqlDataFixture fixture) : SortTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("MySqlDataCollection")]
public class MySql(MySqlDataFixture fixture) : SortTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("MySqlPomeloDataCollection")]
public class MySqlPomelo(MySqlPomeloDataFixture fixture) : SortTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("MongoDbDataCollection")]
public class MongoDb(MongoDbDataFixture fixture) : SortTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}