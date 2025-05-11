namespace MagiQuery.Models;

/// <summary>
/// Used for defining a sorting operation on a specific <see cref="BaseDefinition.Property"/>
/// </summary>
public class SortDefinition: BaseDefinition
{
    /// <summary>
    /// Specifies the order to be used during the sorting operation on the specified
    /// <see cref="BaseDefinition.Property"/>. Unless explicitly defined as <see langword="true"/>, ascending order is
    /// used during the sort.
    /// </summary>
    public bool? Descending { get; set; }
}