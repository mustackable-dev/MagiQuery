using System.ComponentModel.DataAnnotations;

namespace MagiQuery.Models;
/// <summary>
/// A <see cref="QueryRequest"/>-derived request with additional parameters for page
/// size and 1-based page indexing
/// </summary>
public record QueryRequestPaged: QueryRequest
{
    /// <summary>
    /// Represents the page you would like to retrieve. Uses 1-based indexing.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page number should be at least 1.")]
    public int Page { get; } = 1;
    
    /// <summary>
    /// Represents the page size.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page size should be at least 1.")]
    public int PageSize { get; } = 25;
}