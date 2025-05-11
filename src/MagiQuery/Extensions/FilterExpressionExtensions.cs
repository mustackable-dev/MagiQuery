using System.Linq.Expressions;
using MagiQuery.Models;

namespace MagiQuery.Extensions;

internal partial class InternalExtensions
{
    private static Expression ParseFilterExpression(this string? pattern, List<Expression> expressions)
    {
        if(expressions.Count == 1) return expressions[0];

        if (string.IsNullOrWhiteSpace(pattern) || Array.Exists(["&&", "||"], x=>x == pattern))
        {
            Expression mergedExpression = expressions[0];
            for (int i = 1; i < expressions.Count; i++)
            {
                mergedExpression = pattern == "||" ? 
                    Expression.OrElse(mergedExpression, expressions[i]) :
                    Expression.AndAlso(mergedExpression, expressions[i]);
            }
            return mergedExpression;
        }

        string trimmedPattern = new(pattern.Where(x =>
            char.IsDigit(x) ||
            "()!&|".Contains(x)).ToArray());

        if (trimmedPattern.Split('(').Length != trimmedPattern.Split(')').Length ||
            trimmedPattern.Split("&&").Length*2 - 1 != trimmedPattern.Split('&').Length ||
            trimmedPattern.Split("||").Length*2 - 1 != trimmedPattern.Split('|').Length)
        {
            throw new QueryBuildException(QueryBuildExceptionType.MalformedFilterExpression);
        }
            
        string[] tokens = GenerateTokens(trimmedPattern);
        return ParseExpressionNode(tokens, expressions);
    }

    private static string[] GenerateTokens(this string expressionSection)
    {
        List<string> tokens = new();
        int index = 0;
        while(index < expressionSection.Length)
        {
            if (char.IsDigit(expressionSection[index]))
            {
                string expressionIndex = new(expressionSection.Skip(index).TakeWhile(char.IsDigit).ToArray());
                tokens.Add(expressionIndex);
                index+= expressionIndex.Length;
                continue;
            }

            if ("&|()!".Contains(expressionSection[index]))
            {
                int increment = ("&|").Contains(expressionSection[index]) ? 2 : 1;
                tokens.Add(expressionSection.Substring(index, increment));
                index+=increment;
                continue;
            }

            index++;
        }
        return tokens.ToArray();
    }

    private static Expression ParseExpressionNode(string[] tokens, List<Expression> expressions)
    {
        Expression? finalExpression = null;
        bool? andOperator = null;
        bool notOperator = false;
        int index = 0;
        while(index < tokens.Length)
        {
            string token = tokens[index];
            switch (token)
            {
                case "(":
                    int bracketsOpen = 1;
                    string[] subTokens = tokens.Skip(index + 1).TakeWhile(x =>
                    {
                        switch (x)
                        {
                            case "(":
                                bracketsOpen++;
                                break;
                            case ")":
                                bracketsOpen--;
                                break;
                        }
                        return bracketsOpen > 0;
                    }).ToArray();
                    
                    Expression newNode = ParseExpressionNode(subTokens, expressions);
                    finalExpression = JoinNodes(ref newNode, ref finalExpression, ref andOperator, ref notOperator);
                    index += subTokens.Length + 2;
                    break;
                case "&&":
                    andOperator = true;
                    index++;
                    break;
                case "||":
                    andOperator = false;
                    index++;
                    break;
                case "!":
                    notOperator = true;
                    index++;
                    break;
                default:
                    int expressionIndex = int.Parse(token) ;
                    if (expressionIndex < 0 || expressionIndex > expressions.Count - 1)
                        throw new QueryBuildException(
                            QueryBuildExceptionType.IncorrectFilterExpressionIndex, expressionIndex);
                    Expression existingNode = expressions[expressionIndex];
                    finalExpression = JoinNodes(ref existingNode, ref finalExpression, ref andOperator, ref notOperator);
                    index++;
                    break;
            }
        }
        return finalExpression!;
    }

    private static Expression JoinNodes(ref Expression newNode, ref Expression? oldNode, ref bool? andOperator, ref bool notOperator)
    {
        if(notOperator) newNode = Expression.Not(newNode);
        notOperator = false;
        
        Expression result = newNode;
        if (oldNode is not null && andOperator is not null)
        {
            result = andOperator.Value ? Expression.AndAlso(oldNode, newNode) : Expression.OrElse(oldNode, newNode);
        }
        
        return result;
    }
}