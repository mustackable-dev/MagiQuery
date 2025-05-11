namespace MagiQuery.Models;

/// <summary>
/// Used for defining a filtering operation on a specific <see cref="BaseDefinition.Property"/>
/// </summary>
public class FilterDefinition: BaseDefinition
{
    /// <summary>
    /// The operator used for the filter operation on the <see cref="BaseDefinition.Property"/> of the base class. When
    /// choosing an operator, make sure it fits the type of the queried property of the base class
    /// </summary>
    public virtual FilterOperator Operator { get; set; }
    
    /// <summary>
    /// The value to be used in conjunction with the <see cref="FilterDefinition.Operator"/> to carry out the filtering
    /// on the <see cref="BaseDefinition.Property"/> of the base class. Optionally, you can specify
    /// <see cref="BaseDefinition.OverrideCulture"/> and/or <see cref="BaseDefinition.ExactParseFormat"/> in your
    /// <see cref="FilterDefinition"/> to ensure the <see cref="Value"/> is parsed correctly before filtering. If you
    /// want to do <see langword="null"/> evaluation, omit this property from your <see cref="FilterDefinition"/>
    /// initialization.
    /// </summary>
    public string? Value { get; set; }
}