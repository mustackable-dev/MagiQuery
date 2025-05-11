namespace MagiQuery.Models;
/// <summary>
/// A <see cref="QueryRequest"/>-derived request with additional parameters for page
/// size and 1-based page indexing
/// </summary>
public class QueryRequestPaged: QueryRequest
{
    /// <summary>
    /// Represents the page you would like to retrieve. Uses 1-based indexing.
    /// </summary>
    public int Page { get; } = 1;
    
    /// <summary>
    /// Represents the page size.
    /// </summary>
    public int PageSize { get; } = 25;
}