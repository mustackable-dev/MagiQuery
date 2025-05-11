using MagiQuery.Contracts;
using MagiQuery.Models;
using MagiQuery.Translators;

namespace MagiQuery.Factories;

internal static class TranslatorFactory
{
    public static ITranslator CreateTranslator(DataProvider provider)
        => provider switch
        {
            DataProvider.Runtime => new RuntimeTranslator(),
            DataProvider.InMemory => new InMemoryTranslator(),
            DataProvider.Sqlite => new SqliteTranslator(),
            DataProvider.SqlServer => new SqlServerTranslator(),
            DataProvider.PostgreSql => new PostgreSqlTranslator(),
            DataProvider.MySql => new MySqlTranslator(),
            DataProvider.MySqlPomelo => new MySqlPomeloTranslator(),
            DataProvider.MongoDb => new MongoDbTranslator(),
            _ => new RuntimeTranslator()
        };
}