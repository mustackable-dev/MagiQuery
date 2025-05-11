namespace MagiQuery.Models;

/// <summary>
/// This is the base class for a MagiQuery request for generic filtering and sorting
/// </summary>
public class QueryRequest
{
    /// <summary>
    /// This is the collection of <see cref="FilterDefinition"/> to be used during the query build process. Please
    /// remember that the order in which a <see cref="FilterDefinition"/> is defined in this collection determines its
    /// 0-based index number, which in turn is the <see cref="FilterDefinition"/> reference name used in a custom
    /// <see cref="QueryRequest.FilterExpression"/>
    /// </summary>
    public IEnumerable<FilterDefinition>? Filters { get; set; }

    /// <summary>
    /// With this property, you can specify a custom logical expression to be used when applying the specified
    /// <see cref="Filters"/>. The default logical expression in Magiqueery joins all <see cref="FilterDefinition"/>
    /// instances with an <c>AND</c> operator. However, if your application requires a more sophisticated filtering
    /// logic, you can specify it here with the operators <c>&amp;&amp;</c> (AND), <c>||</c> (OR) and <c>!</c> (NOT), 
    /// while using the 0-based index to refer to each individual filter in <see cref="Filters"/>. You can also use
    /// brackets to ensure evaluation happens in the right order.
    /// <see href="https://github.com/mustackable-dev/MagiQuery">Here</see> you can read more about custom logical
    /// expressions
    /// <example>
    /// <code>(0 &amp;&amp; 1) || (2 &amp;&amp; 3)</code>With this expression, only items that satisfy both
    /// <see cref="FilterDefinition"/> with index 0 and  <see cref="FilterDefinition"/> with index 1 OR satisfy both
    /// <see cref="FilterDefinition"/> with index 2 and <see cref="FilterDefinition"/> with index 3 will be returned
    /// </example>
    /// <example>
    /// <code>||</code> With this expression all <see cref="FilterDefinition"/> in <see cref="Filters"/> will be
    /// joined together with the OR operator
    /// </example>
    /// <example>
    /// <code>!0</code>With this expression only items that do not pass the check in the
    /// <see cref="FilterDefinition"/> with index 0 in <see cref="Filters"/> will be returned
    /// </example>
    /// </summary>
    public string? FilterExpression { get; set; }
    
    /// <summary>
    /// This is the collection of <see cref="SortDefinition"/> to be used during the query build process.
    /// </summary>
    public IEnumerable<SortDefinition>? Sort { get; set; }
    
    /// <summary>
    /// With this optional parameter, you can specify the <see cref="System.Globalization.CultureInfo"/> code to be
    /// used during the parsing of all entries in <see cref="Filters"/> and <see cref="Sort"/>.
    /// This property provides a request-wide default value for <see cref="BaseDefinition.OverrideCulture"/>.
    /// You can still override it on a <see cref="BaseDefinition"/> level for a single <see cref="FilterDefinition"/>
    /// or <see cref="SortDefinition"/>. <see cref="System.Globalization.CultureInfo"/> code can be constructed as
    /// explained <see href="https://learn.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo">here</see>.
    /// When the IQueryable source is <see cref="DataProvider.Runtime"/> (and only then),
    /// <see cref="System.Globalization.CultureInfo"/> is also used when formatting the source entries during string
    /// comparison
    /// </summary>
    public string? OverrideCulture { get; set; }
}