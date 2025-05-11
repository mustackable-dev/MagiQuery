using MagiQuery.Models;

namespace MagiQuery.Extensions;

/// <summary>
/// These are convenience extension methods for applying your <see cref="QueryRequest"/> to a given IQueryable.
/// Two utilities methods have been included that return a paged result, which you can directly pass as an endpoint
/// response
/// </summary>
public static class PublicExtensions
{
    /// <summary>
    /// Applies a <see cref="QueryRequest"/> to a given IQueryable with standard <see cref="QueryBuildOptions{T}"/>.
    /// Note that this method only adds the requested filtering and sorting to your IQueryable, it does not generate
    /// a new IEnumerable until you decide to do so. Useful for cases where you need to apply some additional
    /// conditions to your IQueryable before actually querying the data.
    /// </summary>
    /// <param name="source">The IQueryable to apply the request to</param>
    /// <param name="request">The generic filtering and sort request to apply to the source</param>
    /// <returns>An IQueryable you can modify further before calling</returns>
    public static IQueryable<T> ApplyQuery<T>(this IQueryable<T> source, QueryRequest request)
        => source.BuildQuery(request, new());
    
    /// <summary>
    /// Applies a <see cref="QueryRequest"/> to a given IQueryable with an instance of <see cref="QueryBuildOptions{T}"/>.
    /// Note that this method only adds the requested filtering and sorting to your IQueryable, it does not generate a
    /// new IEnumerable until you decide to do so. Useful for cases where you need to apply some additional conditions
    /// to your IQueryable before actually querying the data.
    /// </summary>
    /// <param name="source">The IQueryable to apply the request to</param>
    /// <param name="request">The generic filtering and sort request to apply to the source</param>
    /// <param name="buildOptions">The query build options to use when applying the request to the source</param>
    /// <returns>An IQueryable you can modify further before calling</returns>
    public static IQueryable<T> ApplyQuery<T>(
        this IQueryable<T> source,
        QueryRequest request,
        QueryBuildOptions<T> buildOptions)
        => source.BuildQuery(request, buildOptions);
    
    /// <summary>
    /// A utility extension that runs ApplyQuery with standard <see cref="QueryBuildOptions{T}"/> on a given IQueryable,
    /// executes the query and binds the result to a paged response ready to be served back to the client of an
    /// ASP.NET WebAPI. Takes in a <see cref="QueryRequestPaged"/>, which is a derived class of
    /// <see cref="QueryRequest"/> that takes in page size and 1-based page indexing.
    /// </summary>
    /// <param name="source">The IQueryable to apply the request to</param>
    /// <param name="request">A <see cref="QueryRequest"/>-derived request with additional parameters for page
    /// size and 1-based page indexing</param>
    /// <returns>A utility class instance you can return to the client of a WebAPI</returns>
    public static QueryResponsePaged<T> GetPagedResponse<T>(this IQueryable<T> source, QueryRequestPaged request)
        => new(request, source.BuildQuery(request, new()));
    
    /// <summary>
    /// A utility extension that runs ApplyQuery with an instance of <see cref="QueryBuildOptions{T}"/> on a given
    /// IQueryable, executes the query and binds the result to a paged response ready to be served back to the client
    /// of an ASP.NET WebAPI. Takes in a <see cref="QueryRequestPaged"/>, which is a derived class of
    /// <see cref="QueryRequest"/> that takes in page size and 1-based page indexing.
    /// </summary>
    /// <param name="source">The IQueryable to apply the request to</param>
    /// <param name="request">A <see cref="QueryRequest"/>-derived request with additional parameters for page
    /// size and 1-based page indexing</param>
    /// <param name="buildOptions">The query build options to use when applying the request to the source</param>
    /// <returns>A utility class instance you can return to the client of a WebAPI</returns>
    public static QueryResponsePaged<T> GetPagedResponse<T>(
        this IQueryable<T> source,
        QueryRequestPaged request,
        QueryBuildOptions<T> buildOptions) => new(request, source.BuildQuery(request, buildOptions));
}