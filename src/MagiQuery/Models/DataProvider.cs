namespace MagiQuery.Models;

/// <summary>
/// An enum representing IQueryable source providers
/// </summary>
public enum DataProvider
{
    /// <summary>
    /// The data provider for an IQueryable that has already been loaded fully into memory
    /// </summary>
    Runtime,
    /// <summary>
    /// The data provider for an IQueryable supplied via
    /// <see href="https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.InMemory">
    /// Microsoft.EntityFrameworkCore.InMemory
    /// </see>
    /// </summary>
    InMemory,
    /// <summary>
    /// The data provider for an IQueryable supplied via
    /// <see href="https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Sqlite">
    /// Microsoft.EntityFrameworkCore.Sqlite
    /// </see>
    /// </summary>
    Sqlite,
    /// <summary>
    /// The data provider for an IQueryable supplied via
    /// <see href="https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer">
    /// Microsoft.EntityFrameworkCore.SqlServer
    /// </see>
    /// </summary>
    SqlServer,
    /// <summary>
    /// The data provider for an IQueryable supplied via
    /// <see href="https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL">
    /// Npgsql.EntityFrameworkCore.PostgreSQL
    /// </see>
    /// </summary>
    PostgreSql,
    /// <summary>
    /// The data provider for an IQueryable supplied via
    /// <see href="https://www.nuget.org/packages/MySql.EntityFrameworkCore">
    /// MySql.EntityFrameworkCore
    /// </see>
    /// </summary>
    MySql,
    /// <summary>
    /// The data provider for an IQueryable supplied via
    /// <see href="https://www.nuget.org/packages/Pomelo.EntityFrameworkCore.MySql">
    /// Pomelo.EntityFrameworkCore.MySql
    /// </see>
    /// </summary>
    MySqlPomelo,
    /// <summary>
    /// The data provider for an IQueryable supplied via
    /// <see href="https://www.nuget.org/packages/MongoDB.EntityFrameworkCore">
    /// MongoDB.EntityFrameworkCore
    /// </see>
    /// </summary>
    MongoDb,
}