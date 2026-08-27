using System.ComponentModel.DataAnnotations;

namespace MagiQuery.Models;

/// <summary>
/// A <see cref="QueryRequest"/>-derived request with additional parameters for page
/// size and 1-based page indexing
/// </summary>
public record QueryRequestPaged : QueryRequest
{
    /// <summary>
    /// Represents the page you would like to retrieve. Uses 1-based indexing.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page number should be at least 1.")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Represents the page size.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page size should be at least 1.")]
    public int PageSize { get; set; } = 25;

    /// <summary>
    /// Prevents an extra backend count query when paginating through results.
    /// Set to <see langword="true"/>, if you have already fetched the totals in an initial request.
    /// </summary>
    public bool SkipTotalCalculation { get; set; }
}