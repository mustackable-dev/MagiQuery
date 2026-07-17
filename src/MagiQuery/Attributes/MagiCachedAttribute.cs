using System.Reflection;
using MagiQuery.Utilities;

namespace MagiQuery.Attributes;

/// <summary>
/// Apply this attribute to an entity model to cache its structure at runtime, thereby reducing reflection calls
/// to a minimum
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class MagiCachedAttribute: Attribute
{
    /// <summary>
    /// The depth level to be used when caching the entity's structure
    /// </summary>
    internal int DepthLevel { get; }
    internal BindingFlags PropertyBindingFlags { get; }
    
    /// <summary>
    /// Apply this attribute to an entity model to cache its structure at runtime, thereby reducing reflection calls
    /// to a minimum
    /// <param name="depthLevel">The depth level to be used when caching the entity's structure</param>
    /// <param name="propertyBindingFlags">The <see cref="BindingFlags"/> to be used when enumerating properties
    /// to be cached from the entity. Accepted values range from 1 to 100.</param>
    /// </summary>
    public MagiCachedAttribute(
        int depthLevel = 10,
        BindingFlags propertyBindingFlags = Constants.DefaultPropertyBindingFlags)
    {
        if (depthLevel is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(depthLevel), "Depth level should be between 1 and 100");
        
        DepthLevel = depthLevel;
        PropertyBindingFlags = propertyBindingFlags;
    }
}