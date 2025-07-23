using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using MagiQuery.Contracts;
using MagiQuery.Models;

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
        Type? type = null;
        string[] properties = definition.InternalProperty.Split('.');
        Expression? nullExpression = null;
        Expression memberExpression = parameterExpression;
        bool isNullable = false;
        bool isParentNullable = false;
        
        for(int i=0; i<properties.Length; i++)
        {
            string property = properties[i].ToLower();
            
            type = (type ?? typeof(T)).GetProperty(property, bindingFlags)?.PropertyType;
            if (type is null)
            {
                throw new QueryBuildException(QueryBuildExceptionType.MissingProperty, property);
            }

            isNullable = type.IsInherentlyNullable();
            
            Type? underlyingType = Nullable.GetUnderlyingType(type);
            
            if (underlyingType is not null)
            {
                isNullable = true;
                type = underlyingType;
            }

            if (isNullable)
            {
                isParentNullable = true;
            }
            
            memberExpression = Expression.Property(memberExpression, property);

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
            var type when type == typeof(string) => Expression.Constant(definition.Value, memberType),
            var type when type == typeof(char) => definition.Value.ToCharConstant(isNullable),
            var type when type == typeof(sbyte) => definition.Value.ToSByteConstant(culture, isNullable),
            var type when type == typeof(short) => definition.Value.ToShortConstant(culture, isNullable),
            var type when type == typeof(ushort) => definition.Value.ToUShortConstant(culture, isNullable),
            var type when type == typeof(byte) => definition.Value.ToByteConstant(culture, isNullable),
            var type when type == typeof(int) => definition.Value.ToInt32Constant(culture, isNullable),
            var type when type == typeof(uint) => definition.Value.ToUInt32Constant(culture, isNullable),
            var type when type == typeof(long) => definition.Value.ToInt64Constant(culture, isNullable),
            var type when type == typeof(ulong) => definition.Value.ToUInt64Constant(culture, isNullable),
            var type when type == typeof(nint) => definition.Value.ToIntPtrConstant(culture, isNullable),
            var type when type == typeof(float) => definition.Value.ToFloatConstant(culture, isNullable),
            var type when type == typeof(double) => definition.Value.ToDoubleConstant(culture, isNullable),
            var type when type == typeof(decimal) => definition.Value.ToDecimalConstant(culture, isNullable),
            var type when type == typeof(nuint) => definition.Value.ToUIntPtrConstant(culture, isNullable),
            var type when type == typeof(bool) => definition.Value.ToBoolConstant(isNullable),
            { IsEnum: true } => definition.Value.ToEnumConstant(memberType, isNullable),
            var type when type == typeof(DateTime) => definition.Value.ToDateTimeConstant(culture,
                definition.ExactParseFormat, overrideDateTimeKind, isNullable),
            var type when type == typeof(DateTimeOffset) =>
                definition.Value.ToDateTimeOffsetConstant(culture, definition.ExactParseFormat, isNullable),
            var type when type == typeof(DateOnly) =>
                definition.Value.ToDateOnlyConstant(culture, definition.ExactParseFormat, isNullable),
            var type when type == typeof(TimeSpan) =>
                definition.Value.ToTimeSpanConstant(culture, definition.ExactParseFormat, isNullable),
            var type when type == typeof(TimeOnly) =>
                definition.Value.ToTimeOnlyConstant(culture, definition.ExactParseFormat, isNullable),
            var type when type.IsInherentlyNullable() =>
                Expression.Constant(null, memberType),
            _ => null
        };
    }

    private static bool IsInherentlyNullable(this Type type)
        => !(type.IsValueType || type == typeof(string));

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
            var type when type == typeof(string) => member,
            var type when
                Array.Exists(
                    [
                        typeof(char), typeof(bool), typeof(sbyte), typeof(short), typeof(ushort), typeof(byte),
                        typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(nint), typeof(float),
                        typeof(double), typeof(decimal), typeof(nuint), typeof(DateTime), typeof(DateTimeOffset),
                        typeof(DateOnly), typeof(TimeSpan), typeof(TimeOnly)
                    
                    ], x=>x==type)
                => translator.ForceMemberToString(
                    member,
                    type,
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

    internal class PropertyExpressionComponents
    {
        internal required Type PropertyType { get; init; }
        internal required Expression PropertyExpression { get; init; }
        internal Expression? NullHandlingExpression { get; init; }
        internal bool IsNullable { get; init; }
    }
        
}