namespace MagiQuery.Models;

/// <summary>
/// The base class used for both <see cref="FilterDefinition"/> and <see cref="SortDefinition"/>
/// </summary>
public class BaseDefinition
{
    /// <summary>
    /// The name of the property in your base class that you would like to use in your <see cref="FilterDefinition"/>
    /// or <see cref="SortDefinition"/>. If you would like to access a nested property, you can use "." to go down a
    /// level. If <see cref="QueryBuildOptions{T}.PropertyMapping"/> has been defined, you can use the alias in the map
    /// to refer to the property
    /// </summary>
    public required string Property { get; set; }
    
    /// <summary>
    /// With this optional parameter, you can specify the <see cref="System.Globalization.CultureInfo"/> code to be
    /// used during the parsing of the provided <see cref="FilterDefinition.Value"/>. Also used for processing
    /// <see cref="SortDefinition"/>. A <see cref="System.Globalization.CultureInfo"/> code can be constructed as
    /// explained <see href="https://learn.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo">here</see>.
    /// When the IQueryable source is <see cref="DataProvider.Runtime"/> (and only then),
    /// <see cref="System.Globalization.CultureInfo"/> is also used when formatting the source entries during string
    /// comparison
    /// </summary>
    public string? OverrideCulture { get; set; }
    
    /// <summary>
    /// With this optional parameter, you can specify the exact parse format to be used during the parsing of the
    /// provided  <see cref="FilterDefinition.Value"/>. Also used for processing <see cref="SortDefinition"/>. The
    /// exact source format needs to match the specific type of the property for which it is defined (e.g. "yyyy-MM-dd"
    /// is relevant for <see cref="DateTime"/>, but not for <see cref="TimeSpan"/>). When the IQueryable source is
    /// <see cref="DataProvider.Runtime"/> (and only then), the exact parse format is also used when formatting the
    /// source entries during string comparison
    /// </summary>
    public string? ExactParseFormat { get; set; }
    internal string InternalProperty {get; set;} = string.Empty;
}