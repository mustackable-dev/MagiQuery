using System.Reflection;

namespace MagiQuery.Utilities;

internal static class Constants
{
    internal const BindingFlags DefaultPropertyBindingFlags =
        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;
}