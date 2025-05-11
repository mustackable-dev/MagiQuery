using System.Globalization;
using System.Linq.Expressions;
using MagiQuery.Models;

namespace MagiQuery.Contracts;

internal interface ITranslator
{
    Expression GenerateFilterExpression(
        Expression propertyExpression,
        Expression constantExpression,
        FilterOperator filterOperator,
        ConstantExpression comparer,
        Type propertyType);

    Expression ForceMemberToString(
        Expression member,
        Type T,
        bool isNullable,
        bool isParentNullable,
        CultureInfo culture,
        string? exactParseFormat = null);
}