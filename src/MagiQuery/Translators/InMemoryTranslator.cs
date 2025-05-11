using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using MagiQuery.Cache;

namespace MagiQuery.Translators;

internal class InMemoryTranslator : BaseTranslator
{

    public override Expression ForceMemberToString(
        Expression member,
        Type T,
        bool isNullable,
        bool isParentNullable,
        CultureInfo culture,
        string? exactParseFormat = null)
    {
        if (T == typeof(bool))
        {
            if(isNullable) member = Expression.Coalesce(member, Expression.Constant(false));
            return Expression.Condition(Expression.Equal(member, Expression.Constant(true)),
                Expression.Constant("True"),
                Expression.Constant("False"));
        }
        
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
        
        if (isNullable || isParentNullable)
        {
            // We need to case to nullable in cases where we have a nested non-nullable
            // property inside a nullable parent
            
            if(isParentNullable && !isNullable)
                member = Expression.Convert(member, typeof(Nullable<>).MakeGenericType(T));

            member = Expression.Call(member, "GetValueOrDefault", []);
        }
        
        return Expression.Call(member, method, methodArguments);
    }
}