using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using MagiQuery.Cache;
using MagiQuery.Contracts;
using MagiQuery.Models;
using MagiQuery.Extensions;

namespace MagiQuery.Translators;

internal class RuntimeTranslator : ITranslator
{
    public Expression GenerateFilterExpression(
        Expression propertyExpression,
        Expression constantExpression,
        FilterOperator filterOperator,
        ConstantExpression comparer,
        Type propertyType)
    {
        try
        {
            Expression result = filterOperator switch
            {
                FilterOperator.Equals => propertyType != typeof(string)
                    ? Expression.Equal(propertyExpression, constantExpression)
                    : Expression.Call(
                        propertyExpression,
                        MethodInfoCache.StringComparisonMethods["EqualsWithComparer"],
                        constantExpression,
                        comparer),
                FilterOperator.DoesNotEqual => propertyType != typeof(string)
                    ? Expression.NotEqual(propertyExpression, constantExpression)
                    : Expression.IsFalse(
                        Expression.Call(
                            propertyExpression,
                            MethodInfoCache.StringComparisonMethods["EqualsWithComparer"],
                            constantExpression,
                            comparer)),
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
                        MethodInfoCache.StringComparisonMethods["StartsWithWithComparer"],
                        constantExpression,
                        comparer),
                FilterOperator.EndsWith =>
                    Expression.Call(
                        propertyExpression,
                        MethodInfoCache.StringComparisonMethods["EndsWithWithComparer"],
                        constantExpression,
                        comparer),
                FilterOperator.Contains =>
                    Expression.Call(
                        propertyExpression,
                        MethodInfoCache.StringComparisonMethods["ContainsWithComparer"],
                        constantExpression,
                        comparer),
                FilterOperator.DoesNotContain =>
                    Expression.Not(
                        Expression.Call(
                            propertyExpression,
                            MethodInfoCache.StringComparisonMethods["ContainsWithComparer"],
                            constantExpression,
                            comparer)),
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

    public Expression ForceMemberToString(
        Expression member,
        Type T,
        bool isNullable,
        bool isParentNullable,
        CultureInfo culture,
        string? exactParseFormat = null)
    {
        List<Type> methodArgumentsTypes = new();
        List<Expression> methodArguments = new();
        if (exactParseFormat is not null)
        {
            methodArgumentsTypes.Add(typeof(string));
            methodArguments.Add(Expression.Constant(exactParseFormat));
        }
        else
        {
            //For some reason, as of .Net 9, TimeSpan is missing a "ToString" method
            //that takes only an IFormatProvider as a parameter, so we need to adjust
            //for that
            if (T == typeof(TimeSpan))
            {
                methodArgumentsTypes.Add(typeof(string));
                methodArguments.Add(Expression.Constant(string.Empty));
            }
        }
        
        methodArgumentsTypes.Add(typeof(IFormatProvider));
        methodArguments.Add(Expression.Constant(culture));
        
        MethodInfo method = MethodInfoCache.ToStringMethods[T.Name]
            .First(x=>x.GetParameters()
                .Select(y=>y.ParameterType).SequenceEqual(methodArgumentsTypes));

        if (isNullable)
        {
            member = Expression.Call(member, "GetValueOrDefault", []);
        }
        
        return Expression.Call(member, method, methodArguments);
    }
}