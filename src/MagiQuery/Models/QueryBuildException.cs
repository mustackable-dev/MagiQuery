namespace MagiQuery.Models;

/// <summary>
/// A class for all exceptions thrown during the query build phase
/// </summary>
public class QueryBuildException: Exception
{
    /// <summary>
    /// The type of exception that was thrown
    /// </summary>
    public QueryBuildExceptionType ExceptionType { get; }

    private static readonly Dictionary<QueryBuildExceptionType, string> ErrorMessageTemplates = new()
    {
        {
            QueryBuildExceptionType.MissingProperty, "The property \"{0}\" could not be found. If you are sure " +
                                                     "the property exists, please check your spelling and any " +
                                                     "mapping you may have done via IncludedProperties or " +
                                                     "ExcludedProperties in your QueryBuildOptions. If this " +
                                                     "does not help, consider relaxing the BindingFlags " +
                                                     "restrictions in your QueryBuildOptions to allow for a more " +
                                                     "extensive property search."
        },
        {
            QueryBuildExceptionType.ValueParseError, "The supplied value \"{0}\" for Filter on property \"{1}\" " +
                                                       "could not be parsed into a valid value. Please check " +
                                                       "your formatting, or supply a parsing pattern via the " +
                                                       "\"ExactParseFormat\" field of your Filter or Sort " +
                                                       "expression."
        },
        {
            QueryBuildExceptionType.MalformedFilterExpression, "The supplied FilterExpression is invalid. Please " +
                                                               "check for any unclosed brackets or incorrectly typed " +
                                                               "operators (e.g. & instead of &&)."
        },
        {
            QueryBuildExceptionType.IncorrectFilterExpressionIndex, "Filter with index {0} does not exist. Please " +
                                                                    "keep in mind MagiQuery uses 0-based indexing.\""
        },
        {
            QueryBuildExceptionType.UnsupportedStringComparisonType, "The property type {0} is currently not supported" +
                                                                     " in MagiQuery for string comparisons."
        },
        {
            QueryBuildExceptionType.FilterExpressionGenerationError, "Failed to generate filter expression for " +
                                                                     "property \"{0}\" with type \"{1}\", operator " +
                                                                     "\"{2}\" and constant {3}. Here is the " +
                                                                     "exception message:\n\n{4}."
        },
        {
            QueryBuildExceptionType.SortExpressionGenerationError, "Failed to generate sort expression for " +
                                                                     "property \"{0}\" with type \"{1}\". Here is " +
                                                                     "the exception message:\n\n{2}."
        },
        {
            QueryBuildExceptionType.InvalidOperator, "The operator \"{0}\" is not supported for type \"{1}\"."
        }
    };
    
    internal QueryBuildException(QueryBuildExceptionType type, params object?[] args) :
        base($"{type.ToString()} - {string.Format(ErrorMessageTemplates[type], args)}")
    {
        ExceptionType = type;
    }
}