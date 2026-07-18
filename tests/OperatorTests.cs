using MagiQueryTests.BaseTests;
using MagiQueryTests.Fixtures;
using MagiQueryTests.Fixtures.Implementations;

namespace Operator;

[Collection("RuntimeDataCollection")]
public class Runtime(RuntimeDataFixture fixture) : OperatorTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("InMemoryDataCollection")]
public class InMemory(InMemoryDataFixture fixture) : OperatorTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("SqliteDataCollection")]
public class Sqlite(SqliteDataFixture fixture) : OperatorTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("SqlServerDataCollection")]
public class SqlServer(SqlServerDataFixture fixture) : OperatorTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("PostgreSqlDataCollection")]
public class PostgreSql(PostgreSqlDataFixture fixture) : OperatorTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("MySqlDataCollection")]
public class MySql(MySqlDataFixture fixture) : OperatorTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("MySqlPomeloDataCollection")]
public class MySqlPomelo(MySqlPomeloDataFixture fixture) : OperatorTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}

[Collection("MongoDbDataCollection")]
public class MongoDb(MongoDbDataFixture fixture) : OperatorTests
{
    protected override ITestDataFixture Fixture { get; } = fixture;
}