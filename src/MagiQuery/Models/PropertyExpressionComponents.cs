using System.Linq.Expressions;

namespace MagiQuery.Models;

internal record PropertyExpressionComponents: PropertyComponents
{
    internal required Expression PropertyExpression { get; init; }
    internal Expression? NullHandlingExpression { get; init; }
}