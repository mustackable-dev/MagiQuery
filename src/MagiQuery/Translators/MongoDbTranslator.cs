using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using MagiQuery.Cache;

namespace MagiQuery.Translators;

internal class MongoDbTranslator: BaseTranslator {

    public override Expression ForceMemberToString(
        Expression member,
        Type T,
        bool isNullable,
        bool isParentNullable,
        CultureInfo culture,
        string?exactParseFormat = null)
    {
        string typeName = T.Name;

        if (T == typeof(bool))
        {
            if(isNullable) member = Expression.Coalesce(member, Expression.Constant(false));
            return Expression.Condition(Expression.Equal(member, Expression.Constant(true)),
                Expression.Constant("True"),
                Expression.Constant("False"));
        }
        
        MethodInfo method = MethodInfoCache.ToStringMethods[typeName]
            .First(x=>x.GetParameters()
                .Select(y=>y.ParameterType).SequenceEqual([]));

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

        if (isNullable)
        {
            member = T switch
            {
                _ when T == typeof(DateTimeOffset) =>
                    Expression.Coalesce(member, Expression.Constant(DateTimeOffset.MinValue)),
                _ when T == typeof(DateTime) =>
                    Expression.Coalesce(member, Expression.Constant(DateTime.MinValue)),
                _ when T == typeof(DateOnly) =>
                    Expression.Coalesce(member, Expression.Constant(DateOnly.MinValue)),
                _ when T == typeof(TimeOnly) =>
                    Expression.Coalesce(member, Expression.Constant(TimeOnly.MinValue)),
                _ when T == typeof(TimeSpan) =>
                    Expression.Coalesce(member, Expression.Constant(TimeSpan.MinValue)),
                _ when T == typeof(char) =>
                    Expression.Coalesce(member, Expression.Constant('\0')),
                _ when T == typeof(sbyte) =>
                    Expression.Coalesce(member, Expression.Constant(sbyte.MinValue)),
                _ when T == typeof(byte) =>
                    Expression.Coalesce(member, Expression.Constant(byte.MinValue)),
                _ when T == typeof(short) =>
                    Expression.Coalesce(member, Expression.Constant(short.MinValue)),
                _ when T == typeof(ushort) =>
                    Expression.Coalesce(member, Expression.Constant(ushort.MinValue)),
                _ when T == typeof(uint) =>
                    Expression.Coalesce(member, Expression.Constant(uint.MinValue)),
                _ when T == typeof(long) =>
                    Expression.Coalesce(member, Expression.Constant(long.MinValue)),
                _ when T == typeof(ulong) =>
                    Expression.Coalesce(member, Expression.Constant(ulong.MinValue)),
                _ when T == typeof(decimal) =>
                    Expression.Coalesce(member, Expression.Constant(decimal.MinValue)),
                _ when T == typeof(double) =>
                    Expression.Coalesce(member, Expression.Constant(double.MinValue)),
                _ when T == typeof(float) =>
                    Expression.Coalesce(member, Expression.Constant(float.MinValue)),
                _ => 
                    Expression.Call(member, "GetValueOrDefault", [])
            };
        }
        
        return Expression.Call(member, method);
    }
    
}