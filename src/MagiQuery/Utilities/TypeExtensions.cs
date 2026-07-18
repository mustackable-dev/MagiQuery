namespace MagiQuery.Utilities;

internal static class TypeExtensions
{
    public static bool IsInherentlyNullable(this Type type)
        => !(type.IsValueType || type == typeof(string));
}