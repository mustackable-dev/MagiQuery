using Microsoft.EntityFrameworkCore;

namespace MagiQuery.Models;

/// <summary>
/// A utility class instance you can return to the client of a WebAPI
/// </summary>
public record QueryResponsePaged<T> where T: class
{
    /// <summary>
    /// The result of your query
    /// </summary>
    public IReadOnlyList<T> Data { get; init; }
    /// <summary>
    /// The page indicator for your query, uses 1-based indexing
    /// </summary>
    public int Page { get; init; }
    /// <summary>
    /// The page size used for your query
    /// </summary>
    public int PageSize { get; init; }
    /// <summary>
    /// The total number of pages for your query
    /// </summary>
    public int TotalPages { get; init; }
    /// <summary>
    /// The total number of items in your query
    /// </summary>
    public int TotalItems { get; init; }
    /// <summary>
    /// Indicates if there is a preceding page to the current one in your query
    /// </summary>
    public bool HasPreviousPage { get; init; }
    /// <summary>
    /// Indicates if there is a page after the current one in your query
    /// </summary>
    public bool HasNextPage { get; init; }

    /// <summary>
    /// Use this factory method to generate a response you can return to the client of a WebAPI
    /// </summary>
    internal static QueryResponsePaged<T> Create(QueryRequestPaged request, IQueryable<T> result)
    {
        int totalItems = result.Count();
        int totalPages = (int)Math.Ceiling((double)totalItems / request.PageSize);

        return new()
        {
            Data = result.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            HasPreviousPage = request.Page > 1,
            HasNextPage = request.Page < totalPages,
        };
    }
    
    /// <summary>
    /// Use this async factory method to generate a response you can return to the client of a WebAPI
    /// </summary>
    internal static async Task<QueryResponsePaged<T>> CreateAsync(
        QueryRequestPaged request,
        IQueryable<T> result,
        DataProvider provider)
    {
        int totalItems = 
            provider == DataProvider.Runtime ?
                result.Count() :
                await result
                    .AsNoTracking()
                    .CountAsync();
        
        int totalPages = (int)Math.Ceiling((double)totalItems / request.PageSize);

        IQueryable<T> dataSlice = result.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize);

        return new()
        {
            Data = 
                provider == DataProvider.Runtime ? 
                    dataSlice.ToList() :
                    await dataSlice
                        .AsNoTracking()
                        .ToListAsync(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            HasPreviousPage = request.Page > 1,
            HasNextPage = request.Page < totalPages,
        };
    }
}