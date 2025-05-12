namespace MagiQuery.Models;

/// <summary>
/// Defines the possible exceptions that can be raised during the query build process
/// </summary>
public enum QueryBuildExceptionType
{
    /// <summary>
    /// Thrown when a property could not be resolved
    /// </summary>
    MissingProperty,
    /// <summary>
    /// Thrown when the parsing of the <see cref="FilterDefinition.Value"/> has been unsuccessful
    /// </summary>
    ValueParseError,
    /// <summary>
    /// Thrown when a bad <see cref="QueryRequest.FilterExpression"/> has been provided in the
    /// <see cref="QueryRequest"/>
    /// </summary>
    MalformedFilterExpression,
    /// <summary>
    /// Thrown when an invalid index number was used in a <see cref="QueryRequest.FilterExpression"/>. Please keep in
    /// mind that MagiQuery uses zero-based indexing for <see cref="QueryRequest.Filters"/>
    /// </summary>
    IncorrectFilterExpressionIndex,
    /// <summary>
    /// Thrown when a property type which is not supported has been used for string comparison
    /// </summary>
    UnsupportedStringComparisonType,
    /// <summary>
    /// Thrown when MagiQuery failed to generate an <see cref="System.Linq.Expressions.Expression"/> for a
    /// <see cref="FilterDefinition"/> in <see cref="QueryRequest.Filters"/>
    /// </summary>
    FilterExpressionGenerationError,
    /// <summary>
    /// Thrown when an incompatible pair of operator and property type has been defined in a
    /// <see cref="FilterDefinition"/>
    /// </summary>
    InvalidOperator,
    /// <summary>
    /// Thrown when MagiQuery failed to generate an <see cref="System.Linq.Expressions.Expression"/> for a
    /// <see cref="SortDefinition"/> in <see cref="QueryRequest.Sorts"/>
    /// </summary>
    SortExpressionGenerationError
}