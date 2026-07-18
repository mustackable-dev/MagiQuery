namespace MagiQuery.Models;

internal record PropertyComponents
{
    internal required Type PropertyType { get; set; }
    internal bool IsNullable { get; set; }
}