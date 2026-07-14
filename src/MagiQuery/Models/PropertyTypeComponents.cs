using System.Reflection;

namespace MagiQuery.Models;

internal record PropertyTypeComponents : PropertyComponents
{
    internal required PropertyInfo PropertyInfo { get; init; }
}