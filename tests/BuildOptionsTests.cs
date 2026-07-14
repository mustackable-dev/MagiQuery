using MagiQueryTests.BaseTests;
using MagiQueryTests.Fixtures;
using MagiQueryTests.Fixtures.Implementations;

namespace BuildOptions;

[Collection("RuntimeDataCollection")]
public class Runtime(RuntimeDataFixture fixture) : BuildOptionsTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("InMemoryDataCollection")]
public class InMemory(InMemoryDataFixture fixture) : BuildOptionsTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("SqliteDataCollection")]
public class Sqlite(SqliteDataFixture fixture) : BuildOptionsTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("SqlServerDataCollection")]
public class SqlServer(SqlServerDataFixture fixture) : BuildOptionsTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("PostgreSqlDataCollection")]
public class PostgreSql(PostgreSqlDataFixture fixture) : BuildOptionsTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("MySqlDataCollection")]
public class MySql(MySqlDataFixture fixture) : BuildOptionsTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("MySqlPomeloDataCollection")]
public class MySqlPomelo(MySqlPomeloDataFixture fixture) : BuildOptionsTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("MongoDbDataCollection")]
public class MongoDb(MongoDbDataFixture fixture) : BuildOptionsTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}