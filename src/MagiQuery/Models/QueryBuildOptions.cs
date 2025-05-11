using System.Linq.Expressions;
using System.Reflection;

namespace MagiQuery.Models;

/// <summary>
/// The options to be used when applying a <see cref="QueryRequest"/> to a given IQueryable
/// </summary>
public class QueryBuildOptions<T>
{
    /// <summary>
    /// Here you can specify which properties of the IQueryable's base class you would like to specifically exclude
    /// from the query. If defined, <see cref="QueryBuildOptions{T}.IncludedProperties"/> takes precedence over this
    /// property
    /// </summary>
    public Expression<Func<T, object?>>[]? ExcludedProperties { get; init; }
    
    /// <summary>
    /// Here you can specify all properties of the IQueryable's base class that should be included in your query.
    /// Takes precedence over <see cref="QueryBuildOptions{T}.ExcludedProperties"/>
    /// </summary>
    public Expression<Func<T, object?>>[]? IncludedProperties { get; init; }
    
    /// <summary>
    /// Here you can specify an alias mapping for the IQueryable's base class' properties. Very useful, if you would
    /// like to hide the structure of the IQueryable's base class, or if you just want to add a nickname for a
    /// property which might have a long name (for example, a nested property which goes several levels below the
    /// base class)
    /// </summary>
    public Dictionary<string, Expression<Func<T, object?>>>? PropertyMapping { get; init; } = new();
    
    /// <summary>
    /// Default is <see langword="true"/>. Allows you to control the direct access to properties, for which an alias
    /// has been defined in <see cref="QueryBuildOptions{T}.PropertyMapping"/>. When <see langword="true"/>, a query
    /// can only access a property via its alias (if one has been defined)
    /// </summary>
    public bool HideMappedProperties { get; init; } = true;
    
    /// <summary>
    /// Default is <see cref="DateTimeKind.Unspecified"/>. Allows you to override the DateTimeKind used for
    /// constructing filter constants of <see cref="DateTime"/> and <see cref="DateTimeOffset"/> during the query
    /// definition. Especially useful when using PostgreSQL, as your queries may crash if you use a
    /// <see cref="DateTime"/> with <see cref="DateTimeKind.Unspecified"/> to filter on a column with timestamptz type
    /// </summary>
    public DateTimeKind? OverrideDateTimeKind { get; init; } = DateTimeKind.Unspecified;
    
    /// <summary>
    /// Allows you to specify the <see cref="BindingFlags"/> to be used during the property binding phase of the query
    /// build. <see href="https://learn.microsoft.com/en-us/dotnet/api/system.reflection.bindingflags">Here</see> you
    /// can read more about <see cref="BindingFlags"/>
    /// </summary>
    public BindingFlags PropertyBindingFlags { get; init; } =
        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;
    
    
    /// <summary>
    /// Applicable only to IQueryable instances with source that is already completely loaded in memory
    /// (<see cref="DataProvider.Runtime"/>). Unsupported if you are using a remote source like a database
    /// </summary>
    public StringComparison StringComparisonType { get; init; } = StringComparison.Ordinal;
    internal DataProvider ProviderType { get;  set; } = DataProvider.Runtime;
}