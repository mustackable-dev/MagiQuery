namespace MagiQuery.Models;

/// <summary>
/// A utility class that you can return to a client of your WebAPI
/// </summary>
public class QueryResponsePaged<T>
{
    /// <summary>
    /// The result of your query
    /// </summary>
    public IEnumerable<T> Data { get;}
    /// <summary>
    /// The page indicator for your query, uses 1-based indexing
    /// </summary>
    public int Page { get; }
    /// <summary>
    /// The page size used for your query
    /// </summary>
    public int PageSize { get; }
    /// <summary>
    /// The total number of pages for your query
    /// </summary>
    public int TotalPages { get; }
    /// <summary>
    /// The total number of items in your query
    /// </summary>
    public int TotalItems { get; }
    /// <summary>
    /// Indicates if there is a preceding page to the current one in your query
    /// </summary>
    public bool HasPreviousPage { get; }
    /// <summary>
    /// Indicates if there is a page after the current one in your query
    /// </summary>
    public bool HasNextPage { get; }

    /// <summary>
    /// A utility class that you can return to a client of your WebAPI
    /// </summary>
    public QueryResponsePaged(QueryRequestPaged request, IQueryable<T> result)
    {
        Data = result.Skip((request.Page-1)*request.PageSize).Take(request.PageSize).AsEnumerable();
        Page = request.Page;
        PageSize = request.PageSize;
        TotalItems = result.Count();
        TotalPages = (int)Math.Ceiling((double)TotalItems / request.PageSize);
        HasPreviousPage = Page > 1;
        HasNextPage = Page < TotalPages;
    }
}