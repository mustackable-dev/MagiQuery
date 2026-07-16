using System.Data;
using Dapper;

namespace Benchmark.DAL.Utilities;

public class DateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset>
{
    public override DateTimeOffset Parse(object value) 
        => DateTimeOffset.Parse((string)value);

    public override void SetValue(IDbDataParameter parameter, DateTimeOffset value) 
        => parameter.Value = value.ToString("O");
}