namespace MagiQuery.Models;

/// <summary>
/// Specifes a logic operator to be used during a filtering operation defined in <see cref="FilterDefinition"/>
/// </summary>
public enum FilterOperator
{
    /// <summary>
    /// Relevant for all property types, including nullables.
    /// </summary>
    Equals,
    /// <summary>
    /// Relevant for all property types, including nullables.
    /// </summary>
    DoesNotEqual,
    /// <summary>
    /// Relevant for all property types, except <see cref="System.String"/>, <see cref="System.Char"/> and
    /// <see cref="System.Boolean"/>
    /// </summary>
    GreaterThan,
    /// <summary>
    /// Relevant for all property types, except <see cref="System.String"/>, <see cref="System.Char"/> and
    /// <see cref="System.Boolean"/>
    /// </summary>
    GreaterThanOrEqual,
    /// <summary>
    /// Relevant for all property types, except <see cref="System.String"/>, <see cref="System.Char"/> and
    /// <see cref="System.Boolean"/>
    /// </summary>
    LessThan,
    /// <summary>
    /// Relevant for all property types, except <see cref="System.String"/>, <see cref="System.Char"/> and
    /// <see cref="System.Boolean"/>
    /// </summary>
    LessThanOrEqual,
    /// <summary>
    /// Works natively for properties of type <see cref="System.String"/>. For other types, MagiQuery will attempt to
    /// convert the property value to <see cref="System.String"/> during the filtering operation. When using an
    /// IQueryable with source <see cref="DataProvider.Runtime"/> (and only then),
    /// <see cref="BaseDefinition.OverrideCulture"/> and <see cref="BaseDefinition.ExactParseFormat"/> are also
    /// applied
    /// </summary>
    StartsWith,
    /// <summary>
    /// Works natively for properties of type <see cref="System.String"/>. For other types, MagiQuery will attempt to
    /// convert the property value to <see cref="System.String"/> during the filtering operation. When using an
    /// IQueryable with source <see cref="DataProvider.Runtime"/> (and only then),
    /// <see cref="BaseDefinition.OverrideCulture"/> and <see cref="BaseDefinition.ExactParseFormat"/> are also
    /// applied
    /// </summary>
    EndsWith,
    /// <summary>
    /// Works natively for properties of type <see cref="System.String"/>. For other types, MagiQuery will attempt to
    /// convert the property value to <see cref="System.String"/> during the filtering operation. When using an
    /// IQueryable with source <see cref="DataProvider.Runtime"/> (and only then),
    /// <see cref="BaseDefinition.OverrideCulture"/> and <see cref="BaseDefinition.ExactParseFormat"/> are also
    /// applied
    /// </summary>
    Contains,
    /// <summary>
    /// Works natively for properties of type <see cref="System.String"/>. For other types, MagiQuery will attempt to
    /// convert the property value to <see cref="System.String"/> during the filtering operation. When using an
    /// IQueryable with source <see cref="DataProvider.Runtime"/> (and only then),
    /// <see cref="BaseDefinition.OverrideCulture"/> and <see cref="BaseDefinition.ExactParseFormat"/> are also
    /// applied
    /// </summary>
    DoesNotContain,
    /// <summary>
    /// Applicable only to types <see cref="System.String"/> or <see cref="System.Char"/>. Works exactly as
    /// <see cref="String.IsNullOrEmpty"/>.
    /// </summary>
    IsEmpty,
    /// <summary>
    /// Applicable only to types <see cref="System.String"/> or <see cref="System.Char"/>. Works as an inverse of
    /// <see cref="String.IsNullOrEmpty"/>.
    /// </summary>
    IsNotEmpty,
    /// <summary>
    /// Supported only with certain providers, see <see href="https://github.com/mustackable-dev/MagiQuery">here</see>
    /// for more details. Works natively for properties of type <see cref="System.String"/>. For other types, MagiQuery
    /// will attempt to convert the property value to <see cref="System.String"/> during the filtering operation. When
    /// using an IQueryable with source <see cref="DataProvider.Runtime"/> (and only then),
    /// <see cref="BaseDefinition.OverrideCulture"/> and <see cref="BaseDefinition.ExactParseFormat"/> are also applied
    /// </summary>
    Regex,
}