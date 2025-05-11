using MagiQuery.Extensions;
using MagiQuery.Models;
using MagiQueryTests.Entities;
using MagiQueryTests.Fixtures;

namespace MagiQueryTests;

[Collection("SampleDataCollection")]
public class FilterTests(TestDataFixture fixture)
{
    [Theory]
    [InlineData(null, 5, 13)]
    [InlineData("&&", 5, 13)]
    [InlineData("||", 28, 1)]
    public void ApplyQuery_StandardMultiFilter_ShouldReturnEntry(
        string? filterExpression,
        int expectedCount,
        int expectedId)
    {
        // Arrange
        
        QueryRequest request = new()
        {
            Filters =
            [
                new()
                {
                    Property = "MagicPower",
                    Value = "4600000000",
                    Operator = FilterOperator.GreaterThan,
                },
                new()
                {
                    Property = "Id",
                    Value = "2",
                    Operator = FilterOperator.DoesNotContain,
                },
                new()
                {
                    Property = "DateOfBirth",
                    Value = "2000-01-01",
                    Operator = FilterOperator.LessThan,
                }
            ],
            FilterExpression = filterExpression
        };
        
        // Act
        
        IQueryable<Goblin> query = fixture.SampleData.ApplyQuery(request);
        
        // Assert
        
        Assert.True(query.Count() == expectedCount);
        Assert.True(query.First().Id == expectedId);
    }
    
    [Theory]
    [InlineData("0", 15, 4)]
    [InlineData("!0", 15, 1)]
    [InlineData("0 && 1", 10, 4)]
    [InlineData("0 && !1", 5, 21)]
    [InlineData("!(0 && 1)", 20, 1)]
    [InlineData("!(0 && !1)", 25, 1)]
    [InlineData("2 && 0 && 1", 5, 13)]
    [InlineData("2 && (0 && 1)", 5, 13)]
    [InlineData("(2 && 0) && 1", 5, 13)]
    [InlineData("2 && !0 && 1", 3, 17)]
    [InlineData("2 && (!0 && 1)", 3, 17)]
    [InlineData("2 && !(0 && 1)", 13, 17)]
    [InlineData("!(2 && !(0 && 1))", 17, 1)]
    [InlineData("0 || 1", 23, 1)]
    [InlineData("0 || !1", 22, 2)]
    [InlineData("!(0 || 1)", 7, 2)]
    [InlineData("!(0 || !1)", 8, 1)]
    [InlineData("2 || 0 || 1", 28, 1)]
    [InlineData("2 || (0 || 1)", 28, 1)]
    [InlineData("(2 || 0) || 1", 28, 1)]
    [InlineData("2 || !0 || 1", 30, 1)]
    [InlineData("2 || (!0 || 1)", 30, 1)]
    [InlineData("2 || !(0 || 1)", 20, 2)]
    [InlineData("!(2 || !(0 || 1))", 10, 1)]
    [InlineData("0 && 1 || 2 && 3", 3, 7)]
    [InlineData("(0 && 1) || 2 && 3", 3, 7)]
    [InlineData("((0 && 1) || 2) && 3", 3, 7)]
    [InlineData("0 && (1 || 2) && 3", 2, 7)]
    [InlineData("0 && 1 || (2 && 3)", 11, 4)]
    [InlineData("(!(0 && 3) || (2 || !1)) && 0", 14, 4)]
    public void ApplyQuery_CustomMultiFilter_ShouldReturnEntry(
        string? filterExpression,
        int expectedCount,
        int expectedId)
    {
        // Arrange
        
        QueryRequest request = new()
        {
            Filters =
            [
                new()
                {
                    Property = "MagicPower",
                    Value = "4600000000",
                    Operator = FilterOperator.GreaterThan,
                },
                new()
                {
                    Property = "Id",
                    Value = "2",
                    Operator = FilterOperator.DoesNotContain,
                },
                new()
                {
                    Property = "DateOfBirth",
                    Value = "2000-01-01",
                    Operator = FilterOperator.LessThan,
                },
                new()
                {
                    Property = "Taste",
                    Value = "Sweet",
                    Operator = FilterOperator.Equals,
                }
            ],
            FilterExpression = filterExpression
        };
        
        // Act
        
        IQueryable<Goblin> query = fixture.SampleData.ApplyQuery(request);
        
        // Assert
        
        Assert.True(query.Count() == expectedCount);
        Assert.True(query.First().Id == expectedId);
    }
}