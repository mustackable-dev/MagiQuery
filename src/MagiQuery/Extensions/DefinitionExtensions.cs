using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using MagiQuery.Cache;
using MagiQuery.Contracts;
using MagiQuery.Models;
using MagiQuery.Utilities;

namespace MagiQuery.Extensions;

internal static partial class InternalExtensions
{
    private static Expression ToBooleanExpression<TParent>(
        this FilterDefinition filter,
        ParameterExpression parameter,
        QueryBuildOptions<TParent> options,
        ITranslator translator)
    {
        CultureInfo culture = filter.OverrideCulture is not null ? 
            CultureInfo.GetCultureInfo(filter.OverrideCulture):
            CultureInfo.InvariantCulture;
        
        bool nullEquality = filter.Value is null && filter.Operator == FilterOperator.Equals;
        bool inverseCondition = filter.Value is not null &&
                                Array.Exists(
                                    [FilterOperator.DoesNotEqual, FilterOperator.DoesNotContain],
                                    x => x == filter.Operator);
        
        bool isStringEvaluation =
            Array.Exists(
            [
                FilterOperator.Contains, FilterOperator.StartsWith, FilterOperator.EndsWith,
                FilterOperator.DoesNotContain, FilterOperator.Regex, FilterOperator.IsEmpty,
                FilterOperator.IsNotEmpty
            ], x => x == filter.Operator);
        
        PropertyExpressionComponents components = filter.GetPropertyExpressionComponents<TParent>(
            parameter,
            isStringEvaluation,
            options.PropertyBindingFlags,
            translator,
            nullEquality,
            inverseCondition);
        
        CheckOperatorValidity(filter.Operator, components.PropertyType);
        
        Expression? constantExpression = isStringEvaluation ? 
            Expression.Constant(filter.Value, typeof(string)) : 
            filter.GetConstantExpression(
                culture,
                components.PropertyType,
                components.IsNullable,
                options.OverrideDateTimeKind);

        if (constantExpression is null)
            throw new QueryBuildException(QueryBuildExceptionType.ValueParseError, filter.Value, filter.Property);
            
        Expression filterExpression = translator.GenerateFilterExpression(
            components.PropertyExpression,
            constantExpression,
            filter.Operator,
            Expression.Constant(options.StringComparisonType),
            components.PropertyType);
                
        if (components.NullHandlingExpression is not null)
        {
            filterExpression = !nullEquality && !inverseCondition ?
                Expression.AndAlso(components.NullHandlingExpression, filterExpression) :
                Expression.OrElse(components.NullHandlingExpression, filterExpression);
        }

        return filterExpression;
    }

    private static LambdaExpression ToLambdaPropertyExpression<TParent>(
        this SortDefinition sort,
        PropertyExpressionComponents? components,
        ParameterExpression parameterExpression)
    {
        if (components is null) 
            throw new QueryBuildException(QueryBuildExceptionType.MissingProperty, sort.Property);
        
        Type genericLambdaType = typeof(Func<,>).MakeGenericType(
            typeof(TParent),
            components.IsNullable ? 
                typeof(Nullable<>).MakeGenericType(components.PropertyType):
                components.PropertyType);
        return Expression.Lambda(genericLambdaType, components.PropertyExpression, parameterExpression);
    }

    private static PropertyExpressionComponents GetPropertyExpressionComponents<T>(
        this BaseDefinition definition,
        ParameterExpression parameterExpression,
        bool isStringEvaluation,
        BindingFlags bindingFlags,
        ITranslator translator,
        bool nullEqualityFilter = false,
        bool inverseCondition = false)
    {
        Type type = typeof(T);
        string[] properties = definition.InternalProperty.Split('.');
        Expression? nullExpression = null;
        Expression memberExpression = parameterExpression;
        bool isNullable = false;
        bool isParentNullable = false;

        string cacheKey = type.FullName?.ToLower() ?? "";
        
        for (int i=0; i<properties.Length; i++)
        {
            string property = properties[i].ToLower();
            cacheKey = string.Concat(cacheKey, '.', property);
            
            if(!EntityStructureCache.StructureCache.TryGetValue(cacheKey, out PropertyTypeComponents? typeComponents))
                typeComponents = GetPropertyTypeComponents(property, type, bindingFlags);
            
            if(typeComponents is null)
                throw new QueryBuildException(QueryBuildExceptionType.MissingProperty, definition.InternalProperty);

            isNullable = typeComponents.IsNullable;

            if (isNullable)
            {
                isParentNullable = true;
            }
            
            memberExpression = Expression.Property(memberExpression, typeComponents.PropertyInfo);
            type = typeComponents.PropertyType;

            if (i >= properties.Length - 1) continue;
            
            if (!nullEqualityFilter && !inverseCondition)
            {
                Expression nullEscapeExpression = Expression.NotEqual(memberExpression, Expression.Constant(null));
                nullExpression = nullExpression is not null ? 
                    Expression.AndAlso(nullExpression, nullEscapeExpression) :
                    nullEscapeExpression;
            }
            else
            {
                Expression nullEscapeExpression = Expression.Equal(memberExpression, Expression.Constant(null));
                nullExpression = nullExpression is not null ? 
                    Expression.OrElse(nullExpression, nullEscapeExpression) :
                    nullEscapeExpression;
            }
        }
        
        return new()
        {
            PropertyType = type!,
            PropertyExpression = isStringEvaluation ? 
                memberExpression.ForceNonStringExpressionToString(
                    definition,
                    type!,
                    isNullable,
                    isParentNullable,
                    translator) :
                memberExpression,
            NullHandlingExpression = nullExpression,
            IsNullable = isNullable
        };
    }

    private static PropertyTypeComponents? GetPropertyTypeComponents(string property, Type parentType, BindingFlags flags)
    {
        PropertyInfo? propertyInfo = parentType.GetProperty(property, flags | BindingFlags.IgnoreCase);
        
        if (propertyInfo is null)
            return null;

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

    private static ConstantExpression? GetConstantExpression(
        this FilterDefinition definition,
        CultureInfo culture,
        Type memberType,
        bool isNullable,
        DateTimeKind? overrideDateTimeKind)
    {
        if(definition.Value is null)
        {
            return Expression.Constant(null);
        }

        return memberType switch
        {
            _ when memberType == typeof(string) => Expression.Constant(definition.Value, memberType),
            _ when memberType == typeof(char) => definition.Value.ToCharConstant(isNullable),
            _ when memberType == typeof(sbyte) => definition.Value.ToSByteConstant(culture, isNullable),
            _ when memberType == typeof(short) => definition.Value.ToShortConstant(culture, isNullable),
            _ when memberType == typeof(ushort) => definition.Value.ToUShortConstant(culture, isNullable),
            _ when memberType == typeof(byte) => definition.Value.ToByteConstant(culture, isNullable),
            _ when memberType == typeof(int) => definition.Value.ToInt32Constant(culture, isNullable),
            _ when memberType == typeof(uint) => definition.Value.ToUInt32Constant(culture, isNullable),
            _ when memberType == typeof(long) => definition.Value.ToInt64Constant(culture, isNullable),
            _ when memberType == typeof(ulong) => definition.Value.ToUInt64Constant(culture, isNullable),
            _ when memberType == typeof(nint) => definition.Value.ToIntPtrConstant(culture, isNullable),
            _ when memberType == typeof(float) => definition.Value.ToFloatConstant(culture, isNullable),
            _ when memberType == typeof(double) => definition.Value.ToDoubleConstant(culture, isNullable),
            _ when memberType == typeof(decimal) => definition.Value.ToDecimalConstant(culture, isNullable),
            _ when memberType == typeof(nuint) => definition.Value.ToUIntPtrConstant(culture, isNullable),
            _ when memberType == typeof(bool) => definition.Value.ToBoolConstant(isNullable),
            { IsEnum: true } => definition.Value.ToEnumConstant(memberType, isNullable),
            _ when memberType == typeof(DateTime) => definition.Value.ToDateTimeConstant(culture,
                definition.ExactParseFormat, overrideDateTimeKind, isNullable),
            _ when memberType == typeof(DateTimeOffset) =>
                definition.Value.ToDateTimeOffsetConstant(culture, definition.ExactParseFormat, isNullable),
            _ when memberType == typeof(DateOnly) =>
                definition.Value.ToDateOnlyConstant(culture, definition.ExactParseFormat, isNullable),
            _ when memberType == typeof(TimeSpan) =>
                definition.Value.ToTimeSpanConstant(culture, definition.ExactParseFormat, isNullable),
            _ when memberType == typeof(TimeOnly) =>
                definition.Value.ToTimeOnlyConstant(culture, definition.ExactParseFormat, isNullable),
            _ when memberType.IsInherentlyNullable() =>
                Expression.Constant(null, memberType),
            _ => null
        };
    }

    private static void CheckOperatorValidity(FilterOperator filterOperator, Type propertyType)
    {
        //Quantitative comparisons
        
        if (Array.Exists(
                [
                    FilterOperator.GreaterThan, FilterOperator.GreaterThanOrEqual,
                    FilterOperator.LessThan, FilterOperator.LessThanOrEqual
                ],
                x => x == filterOperator) &&
            Array.Exists(
                [
                    typeof(string), typeof(char), typeof(char?), typeof(bool), typeof(bool?)
                ],
                x => x == propertyType
            )
        )
        {
            throw new QueryBuildException(
                QueryBuildExceptionType.InvalidOperator,
                filterOperator.ToString(),
                propertyType.Name);
        }
        
        //String only comparisons
        
        if(
            Array.Exists([FilterOperator.IsEmpty, FilterOperator.IsNotEmpty], x=>x==filterOperator) &&
            !Array.Exists([typeof(string), typeof(char?), typeof(char)], x=>x==propertyType)
            )
        {
            throw new QueryBuildException(
                QueryBuildExceptionType.InvalidOperator,
                filterOperator.ToString(),
                propertyType.Name);
        }
    }

    private static Expression ForceNonStringExpressionToString(
        this Expression member,
        BaseDefinition definition,
        Type memberType,
        bool isNullable,
        bool isParentNullable,
        ITranslator translator)
    {
        CultureInfo culture = definition.OverrideCulture is not null ? 
            CultureInfo.GetCultureInfo(definition.OverrideCulture):
            CultureInfo.InvariantCulture;

        return memberType switch
        {
            _ when memberType == typeof(string) => member,
            _ when
                Array.Exists(
                    [
                        typeof(char), typeof(bool), typeof(sbyte), typeof(short), typeof(ushort), typeof(byte),
                        typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(nint), typeof(float),
                        typeof(double), typeof(decimal), typeof(nuint), typeof(DateTime), typeof(DateTimeOffset),
                        typeof(DateOnly), typeof(TimeSpan), typeof(TimeOnly)
                    
                    ], x=>x== memberType)
                => translator.ForceMemberToString(
                    member,
                    memberType,
                    isNullable,
                    isParentNullable,
                    culture,
                    definition.ExactParseFormat),
            
            { IsEnum: true }
                => translator.ForceMemberToString(
                    member,
                    typeof(Enum),
                    isNullable,
                    isParentNullable,
                    culture,
                    definition.ExactParseFormat),
            
            _ => throw new QueryBuildException(QueryBuildExceptionType.UnsupportedStringComparisonType, memberType.ToString())
        };
    }
}