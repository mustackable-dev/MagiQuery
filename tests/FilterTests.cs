using MagiQueryTests.BaseTests;
using MagiQueryTests.Fixtures;
using MagiQueryTests.Fixtures.Implementations;

namespace Filter;

[Collection("RuntimeDataCollection")]
public class Runtime(RuntimeDataFixture fixture) : FilterTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("InMemoryDataCollection")]
public class InMemory(InMemoryDataFixture fixture) : FilterTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("SqliteDataCollection")]
public class Sqlite(SqliteDataFixture fixture) : FilterTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("SqlServerDataCollection")]
public class SqlServer(SqlServerDataFixture fixture) : FilterTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("PostgreSqlDataCollection")]
public class PostgreSql(PostgreSqlDataFixture fixture) : FilterTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("MySqlDataCollection")]
public class MySql(MySqlDataFixture fixture) : FilterTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("MySqlPomeloDataCollection")]
public class MySqlPomelo(MySqlPomeloDataFixture fixture) : FilterTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("MongoDbDataCollection")]
public class MongoDb(MongoDbDataFixture fixture) : FilterTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}