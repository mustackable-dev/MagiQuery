using System.Collections.Frozen;
using System.Reflection;
using MagiQuery.Attributes;
using MagiQuery.Models;
using MagiQuery.Utilities;

namespace MagiQuery.Cache;

internal static class EntityStructureCache
{
    internal static FrozenDictionary<string, PropertyTypeComponents> StructureCache { get; }
        = BuildStructureCache().ToFrozenDictionary();

    private static Dictionary<string, PropertyTypeComponents> BuildStructureCache()
    {
        Dictionary<string, PropertyTypeComponents> result = new();
        foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()))
        {
            CachedAttribute? cachedAttribute = type.GetCustomAttribute<CachedAttribute>();
            
            if(cachedAttribute is null)
                continue;

            string cacheBaseKey = type.FullName?.ToLower() ?? "";
            
            foreach (PropertyInfo property in type.GetProperties(cachedAttribute.PropertyBindingFlags))
            {
                int depthLevel = 0;
                
                GeneratePropertyTypeComponents(
                    property,
                    cacheBaseKey,
                    cachedAttribute.DepthLevel,
                    cachedAttribute.PropertyBindingFlags,
                    ref result,
                    ref depthLevel);
            }
        }
        return result;
    }

    private static void GeneratePropertyTypeComponents(
        this PropertyInfo propertyInfo,
        string baseCacheKey,
        int depthLevelLimit,
        BindingFlags flags,
        ref Dictionary<string, PropertyTypeComponents> collector,
        ref int depthLevelCounter)
    {
        if (depthLevelCounter == depthLevelLimit)
            return;
        
        depthLevelCounter++;
        
        string cacheKey = string.Concat(baseCacheKey, '.', propertyInfo.Name.ToLower());
        PropertyTypeComponents components = propertyInfo.GetPropertyTypeComponents();
        collector.TryAdd(cacheKey, components);

        if (!components.PropertyType.IsClass || components.PropertyType == typeof(string))
            return;

        foreach (PropertyInfo property in components.PropertyType.GetProperties(flags))
        {
            int localDepthLevel = depthLevelCounter;
            //Endless DateTime.Date loop needs to be broken
            if (property.PropertyType == components.PropertyType)
                continue;
            
            property.GeneratePropertyTypeComponents(
                cacheKey,
                depthLevelLimit,
                flags,
                ref collector,
                ref localDepthLevel);
        }
    }
    
    private static PropertyTypeComponents GetPropertyTypeComponents(this PropertyInfo propertyInfo)
    {
        PropertyTypeComponents result = new()
        {
            PropertyInfo = propertyInfo,
            PropertyType = propertyInfo.PropertyType,
            IsNullable = propertyInfo.PropertyType.IsInherentlyNullable()
        };
        
        Type? underlyingType = Nullable.GetUnderlyingType(result.PropertyType);
        
        if (underlyingType is null)
            return result;
        
        result.IsNullable = true;
        result.PropertyType = underlyingType;

        return result;
    }
    
}