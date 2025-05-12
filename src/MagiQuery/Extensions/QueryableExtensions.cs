using System.Linq.Expressions;
using MagiQuery.Cache;
using MagiQuery.Contracts;
using MagiQuery.Factories;
using MagiQuery.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace MagiQuery.Extensions;

internal static partial class InternalExtensions
{
    internal static IQueryable<T> BuildQuery<T>(this IQueryable<T> queryable, QueryRequest request, QueryBuildOptions<T> options)
    {
        request.ApplyMapping(options);
        request.ApplyPropertyScreening(options);
        
        ResolveSourceType(queryable, options);

        ITranslator translator = TranslatorFactory.CreateTranslator(options.ProviderType);
        
        queryable = queryable.ApplyFilters(request, options, translator)
                             .ApplySorts(request, options, translator);
        
        return queryable;
    }

    private static IQueryable<T> ApplyFilters<T>(
        this IQueryable<T> source,
        QueryRequest request,
        QueryBuildOptions<T> options,
        ITranslator translator)
    {
        if (request.Filters is null || !request.Filters.Any()) return source;
        
        List<Expression> filterConditionals = new();
        
        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
        
        foreach (FilterDefinition filter in request.Filters)
        {
            if(filter.OverrideCulture is null && request.OverrideCulture is not null)
                filter.OverrideCulture = request.OverrideCulture;
            
            filterConditionals.Add(filter.ToBooleanExpression(parameter, options, translator));
        }
        
        source = source.Where(Expression.Lambda<Func<T, bool>>(
            request.FilterExpression.ParseFilterExpression(filterConditionals),
            parameter));

        return source;
    }

    private static IQueryable<T> ApplySorts<T>(
        this IQueryable<T> source,
        QueryRequest request,
        QueryBuildOptions<T> options,
        ITranslator translator)
    {
        if (request.Sorts is null || !request.Sorts.Any()) return source;
        
        bool firstOrderDone = false;
        IOrderedQueryable<T> ordered = null!;
        
        StringComparer stringComparer = options.StringComparisonType.ToStringComparer();
        foreach (SortDefinition sort in request.Sorts)
        {
            sort.OverrideCulture ??= request.OverrideCulture;

            ParameterExpression parameterExpression = Expression.Parameter(typeof(T), "x");
            PropertyExpressionComponents components = sort.GetPropertyExpressionComponents<T>(
                parameterExpression,
                false,
                options.PropertyBindingFlags,
                translator);

            try
            {
                LambdaExpression orderExpression =
                    sort.ToLambdaPropertyExpression<T>(components, parameterExpression);

                List<object?> payload = new([orderExpression]);
                bool canUseStringComparer = components.PropertyType == typeof(string) &&
                                            //StringComparer sorts are not available in Entity Framework
                                            options.ProviderType is DataProvider.Runtime;

                string methodName = $"{(!firstOrderDone ? "Order" : "Then")}By" +
                                    $"{(sort.Descending ?? false ? "Descending" : string.Empty)}" +
                                    $"{(canUseStringComparer ? "String" : string.Empty)}";

                if (canUseStringComparer) payload.Add(stringComparer);
                payload.Insert(0, !firstOrderDone ? source : ordered);

                ordered = (IOrderedQueryable<T>)
                    MethodInfoCache.SortMethods[methodName]
                        .MakeGenericMethod(typeof(T), orderExpression.ReturnType)
                        .Invoke(null, payload.ToArray())!;
                firstOrderDone = true;
            }
            catch (Exception ex)
            {
                throw new QueryBuildException(
                    QueryBuildExceptionType.SortExpressionGenerationError,
                    sort.Property,
                    components.PropertyType.ToString(),
                    ex.Message);
            }
        }

        return ordered;
    }
    
    private static StringComparer ToStringComparer(this StringComparison stringComparison) =>
        stringComparison switch
        {
            StringComparison.Ordinal => StringComparer.Ordinal,
            StringComparison.OrdinalIgnoreCase => StringComparer.OrdinalIgnoreCase,
            StringComparison.InvariantCulture => StringComparer.InvariantCulture,
            StringComparison.InvariantCultureIgnoreCase => StringComparer.InvariantCultureIgnoreCase,
            StringComparison.CurrentCulture => StringComparer.CurrentCulture,
            StringComparison.CurrentCultureIgnoreCase => StringComparer.CurrentCultureIgnoreCase,
            _=> StringComparer.Ordinal
        };

    private static void ApplyMapping<T>(this QueryRequest request, QueryBuildOptions<T> options)
    {
        Dictionary<string, string>? mappingDictionary =
            options.PropertyMapping?.ToDictionary(x => x.Key, x => x.Value.ParsePropertyName());
        HashSet<string>? mappedProperties = mappingDictionary?.Values.ToHashSet();
        
        if(request.Filters is not null && request.Filters.Any()) MapBaseDefinitionsProperties(
            request.Filters,
            mappingDictionary,
            mappedProperties,
            options.HideMappedProperties);
            
        if(request.Sorts is not null && request.Sorts.Any()) MapBaseDefinitionsProperties(
            request.Sorts,
            mappingDictionary,
            mappedProperties,
            options.HideMappedProperties);
    }

    private static void MapBaseDefinitionsProperties(
        IEnumerable<BaseDefinition> definitions,
        Dictionary<string, string>? mappingDictionary,
        HashSet<string>? mappedProperties,
        bool hideMappedProperties)
    {
        foreach (BaseDefinition definition in definitions)
        {
            if (mappingDictionary is not null)
            {
                if (mappingDictionary.TryGetValue(definition.Property, out string? value))
                {
                    definition.InternalProperty = value;
                    continue;
                }
                if (hideMappedProperties && mappedProperties!.Contains(definition.Property))
                {
                    throw new QueryBuildException(QueryBuildExceptionType.MissingProperty,definition.Property);
                }
            }
            definition.InternalProperty = definition.Property;
        }
    }

    private static void ApplyPropertyScreening<T>(this QueryRequest request, QueryBuildOptions<T> options)
    {
        bool includedScreening = options.IncludedProperties?.Any() ?? false;
        
        if (!includedScreening && options.ExcludedProperties is null) return;
        
        IEnumerable<string> propertiesChecklist = includedScreening ?
            options.IncludedProperties!.Select(x =>x.ParsePropertyName()):
            options.ExcludedProperties!.Select(x =>x.ParsePropertyName());
            
        if (request.Filters is not null)
        {
            FilterDefinition? match = request.Filters.FirstOrDefault(x => includedScreening ?
                !propertiesChecklist.Contains(x.InternalProperty):
                propertiesChecklist.Contains(x.InternalProperty));
            if (match is not null)
                throw new QueryBuildException(QueryBuildExceptionType.MissingProperty, match.Property);
        }

        if (request.Sorts is not null)
        {
            SortDefinition? match = request.Sorts.FirstOrDefault(x => includedScreening ?
                !propertiesChecklist.Contains(x.InternalProperty):
                propertiesChecklist.Contains(x.InternalProperty));
            if (match is not null)
                throw new QueryBuildException(QueryBuildExceptionType.MissingProperty, match.Property);
        }
    }

    private static string ParsePropertyName<T>(this Expression<Func<T, object?>> expression)
    {
        string rawExpression = expression.Body is UnaryExpression { Operand: MemberExpression propertyExpression } ? 
            propertyExpression.ToString():
            expression.ToString();
        
        return rawExpression[(rawExpression.IndexOf('.') + 1)..];
    }

    private static void ResolveSourceType<T>(this IQueryable source, QueryBuildOptions<T> options)
    {
        string providerName = string.Empty;
        if (source is IInfrastructure<DbContext> context)
        {
            providerName = context.Instance.Database.ProviderName ?? string.Empty;
        }

        options.ProviderType = providerName switch
        {
            "Microsoft.EntityFrameworkCore.InMemory" => DataProvider.InMemory,
            "Microsoft.EntityFrameworkCore.Sqlite" => DataProvider.Sqlite,
            "Microsoft.EntityFrameworkCore.SqlServer" => DataProvider.SqlServer,
            "Npgsql.EntityFrameworkCore.PostgreSQL" => DataProvider.PostgreSql,
            "MySql.EntityFrameworkCore" => DataProvider.MySql,
            "Pomelo.EntityFrameworkCore.MySql" => DataProvider.MySqlPomelo,
            "MongoDB.EntityFrameworkCore" => DataProvider.MongoDb,
            _ => DataProvider.Runtime
        };
    }
}