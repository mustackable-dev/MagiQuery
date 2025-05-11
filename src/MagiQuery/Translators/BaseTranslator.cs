using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using MagiQuery.Cache;
using MagiQuery.Contracts;
using MagiQuery.Models;
using MagiQuery.Extensions;

namespace MagiQuery.Translators;

internal class BaseTranslator: ITranslator
{
    public virtual Expression GenerateFilterExpression(Expression propertyExpression, Expression constantExpression,
        FilterOperator filterOperator, ConstantExpression comparer, Type propertyType)
    {
        try
        {
            Expression result = filterOperator switch
            {
                FilterOperator.Equals => Expression.Equal(propertyExpression, constantExpression),
                FilterOperator.DoesNotEqual => Expression.NotEqual(propertyExpression, constantExpression),
                FilterOperator.GreaterThan => 
                    Expression.GreaterThan(
                        propertyExpression.ConvertToIntIfEnum(propertyType),
                        constantExpression.ConvertToIntIfEnum(propertyType)),
                FilterOperator.LessThan => 
                    Expression.LessThan(
                        propertyExpression.ConvertToIntIfEnum(propertyType),
                        constantExpression.ConvertToIntIfEnum(propertyType)),
                FilterOperator.GreaterThanOrEqual =>
                    Expression.GreaterThanOrEqual(
                        propertyExpression.ConvertToIntIfEnum(propertyType),
                        constantExpression.ConvertToIntIfEnum(propertyType)),
                FilterOperator.LessThanOrEqual => 
                    Expression.LessThanOrEqual(
                        propertyExpression.ConvertToIntIfEnum(propertyType),
                        constantExpression.ConvertToIntIfEnum(propertyType)),
                FilterOperator.StartsWith =>
                    Expression.Call(
                        propertyExpression,
                        MethodInfoCache.StringComparisonMethods["StartsWith"],
                        constantExpression),
                FilterOperator.EndsWith =>
                    Expression.Call(
                        propertyExpression,
                        MethodInfoCache.StringComparisonMethods["EndsWith"],
                        constantExpression),
                FilterOperator.Contains =>
                    Expression.Call(
                        propertyExpression,
                        MethodInfoCache.StringComparisonMethods["Contains"],
                        constantExpression),
                FilterOperator.DoesNotContain =>
                    Expression.Not(
                        Expression.Call(
                            propertyExpression,
                            MethodInfoCache.StringComparisonMethods["Contains"],
                            constantExpression)),
                FilterOperator.IsEmpty =>
                    Expression.Call(MethodInfoCache.StringComparisonMethods["IsNullOrWhiteSpace"], propertyExpression),
                FilterOperator.IsNotEmpty =>
                    Expression.Not(Expression.Call(MethodInfoCache.StringComparisonMethods["IsNullOrWhiteSpace"],
                        propertyExpression)),
                FilterOperator.Regex =>
                    Expression.Call(MethodInfoCache.RegexMethods["IsMatch"], propertyExpression, constantExpression),
                _ => throw new ArgumentOutOfRangeException(nameof(filterOperator), filterOperator, null)
            };
            return result;
        }
        catch (Exception ex)
        {
            throw new QueryBuildException(
                QueryBuildExceptionType.FilterExpressionGenerationError,
                propertyExpression.ToString(),
                propertyType.ToString(),
                filterOperator.ToString(),
                constantExpression.ToString(),
                ex.Message);
        }
    }

    public virtual Expression ForceMemberToString(
        Expression member,
        Type T,
        bool isNullable,
        bool isParentNullable,
        CultureInfo culture,
        string? exactParseFormat = null)
    {
        string typeName = T.Name;

        if (T == typeof(bool))
        {
            if(isNullable) member = Expression.Coalesce(member, Expression.Constant(false));
            return Expression.Condition(Expression.Equal(member, Expression.Constant(true)),
                Expression.Constant("True"),
                Expression.Constant("False"));
        }

        if (T == typeof(Enum))
        {
            var enumMembers = Enum.GetValues(isNullable ? Nullable.GetUnderlyingType(member.Type)!: member.Type);
            
            if (enumMembers.Length > 0)
            {
                Expression enumExpression = Expression.Constant(string.Empty);
                member = Expression.Convert(member, typeof(int));
                for (int i = enumMembers.Length-1; i >=0; i--)
                {
                    var enumMember = enumMembers.GetValue(i);
                    enumExpression = Expression.Condition(
                        Expression.Equal(member, Expression.Constant((int)enumMember!)),
                        Expression.Constant(enumMember.ToString()), enumExpression);
                }
                return enumExpression;
            }
        }
        
        MethodInfo method = MethodInfoCache.ToStringMethods[typeName]
            .First(x=>x.GetParameters()
                .Select(y=>y.ParameterType).SequenceEqual([]));

        if (isNullable)
        {
            member = T switch
            {
                var t when t == typeof(DateTimeOffset) =>
                    Expression.Coalesce(member, Expression.Constant(DateTimeOffset.MinValue)),
                var t when t == typeof(DateTime) =>
                    Expression.Coalesce(member, Expression.Constant(DateTime.MinValue)),
                var t when t == typeof(DateOnly) =>
                    Expression.Coalesce(member, Expression.Constant(DateOnly.MinValue)),
                var t when t == typeof(TimeOnly) =>
                    Expression.Coalesce(member, Expression.Constant(TimeOnly.MinValue)),
                var t when t == typeof(TimeSpan) =>
                    Expression.Coalesce(member, Expression.Constant(TimeSpan.Zero)),
                var t when t == typeof(char) =>
                    Expression.Coalesce(member, Expression.Constant('\0')),
                _ => 
                    Expression.Call(member, "GetValueOrDefault", [])
            };
        }
        
        return Expression.Call(member, method);
    }
}