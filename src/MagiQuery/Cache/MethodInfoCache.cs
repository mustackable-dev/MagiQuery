using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.Frozen;

namespace MagiQuery.Cache;

internal static class MethodInfoCache
{
    internal static FrozenDictionary<string, MethodInfo> SortMethods { get; } = typeof(Queryable)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(x => Array.Exists(["OrderBy", "OrderByDescending", "ThenBy", "ThenByDescending"], y=>y==x.Name) &&
                    (
                        x.GetParameters()
                            .Select(y=>y.ParameterType.GetGenericTypeDefinition())
                            .SequenceEqual([typeof(IOrderedQueryable<>), typeof(Expression<>), typeof(IComparer<>)]) ||
                        x.GetParameters()
                            .Select(y=>y.ParameterType.GetGenericTypeDefinition())
                            .SequenceEqual([typeof(IQueryable<>), typeof(Expression<>), typeof(IComparer<>)]) ||
                        x.GetParameters()
                            .Select(y=>y.ParameterType.GetGenericTypeDefinition())
                            .SequenceEqual([typeof(IOrderedQueryable<>), typeof(Expression<>)]) ||
                        x.GetParameters()
                            .Select(y=>y.ParameterType.GetGenericTypeDefinition())
                            .SequenceEqual([typeof(IQueryable<>), typeof(Expression<>)])
                    ))
        .ToFrozenDictionary(x => $"{x.Name}{(x.GetParameters().Length == 3 ? "String" : string.Empty)}", x=>x);
    
    internal static FrozenDictionary<string, MethodInfo> StringComparisonMethods { get; } = typeof(string)
        .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
        .Where(x => 
            (
                Array.Exists(["Equals", "StartsWith", "EndsWith", "Contains"], y=>y==x.Name) &&
                x.GetParameters()
                    .Select(y=>y.ParameterType)
                    .SequenceEqual([typeof(string)])
            ) ||
            (
                Array.Exists(["Equals", "StartsWith", "EndsWith", "Contains"], y=>y==x.Name) &&
                x.GetParameters()
                    .Select(y=>y.ParameterType)
                    .SequenceEqual([typeof(string), typeof(StringComparison)])
            ) ||
                Array.Exists(["IsNullOrWhiteSpace"], y=>y==x.Name)
            )
        .ToFrozenDictionary(
            x => $"{x.Name}{(x.GetParameters().Any(y=>y.ParameterType == typeof(StringComparison)) ?
                "WithComparer":
                string.Empty)}",
            x => x);
    
    internal static FrozenDictionary<string, MethodInfo> RegexMethods { get; } = typeof(Regex)
        .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
        .Where(x => 
            Array.Exists(["IsMatch"], y=>y==x.Name) &&
            x.GetParameters()
                .Select(y=>y.ParameterType)
                .SequenceEqual([typeof(string), typeof(string)]))
        .ToFrozenDictionary(x => x.Name, x => x);

    internal static FrozenDictionary<string, IEnumerable<MethodInfo>> ToStringMethods { get; } =
        new List<Type>([
                typeof(string), typeof(char), typeof(sbyte), typeof(short), typeof(ushort), typeof(byte), typeof(int),
                typeof(uint), typeof(long), typeof(ulong), typeof(nint), typeof(float), typeof(double), typeof(decimal),
                typeof(nuint), typeof(DateTime), typeof(DateTimeOffset), typeof(DateOnly), typeof(TimeOnly), 
                typeof(TimeSpan), typeof(bool), typeof(Enum)
            ])
            .ToFrozenDictionary(x => x.Name, y => y
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(z => z.Name == "ToString"));
}