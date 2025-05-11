using System.Linq.Expressions;
using System.Reflection;
using MagiQueryTests.Fixtures;
using MagiQueryTests.Entities;
using MagiQuery.Models;
using MagiQuery.Extensions;

namespace MagiQueryTests;

[Collection("SampleDataCollection")]
public class BuildOptionsTests(TestDataFixture fixture)
{
    [Theory]
    [InlineData(StringComparison.Ordinal, 0)]
    [InlineData(StringComparison.OrdinalIgnoreCase, 1)]
    public void ApplyQuery_StringComparisonTypeOption_ShouldReturnEntryCountOnFilter(
        StringComparison comparisonType,
        int expectedCount)
    {
        // Arrange
        
        QueryRequest request = new()
        {
            Filters =
            [
                new()
                {
                    Property = "Name",
                    Value = "blee",
                    Operator = FilterOperator.Contains,
                }
            ]
        };

        QueryBuildOptions<Goblin> options = new()
        {
            StringComparisonType = comparisonType,
        };
        
        // Act
        
        IQueryable<Goblin> query = fixture.SampleData.ApplyQuery(request, options);
        
        // Assert
        
        Assert.SkipWhen(TestDataFixture.Provider != DataProvider.Runtime,
            "Order by clauses with StringComparer are not available in SQL EF Core");
        
        Assert.True(query.Count() == expectedCount);
    }
    
    [Theory]
    [InlineData(StringComparison.Ordinal, "Name", false, 3)]
    [InlineData(StringComparison.OrdinalIgnoreCase, "Name", false, 10)]
    [InlineData(StringComparison.OrdinalIgnoreCase, "Id", true, 17)]
    public void ApplyQuery_StringComparisonTypeOption_ShouldReturnEntryWithIdAtIndexOnSort(
        StringComparison comparisonType,
        string propertyName,
        bool descending,
        int expectedId)
    {
        // Arrange
        
        QueryRequest request = new()
        {
            Sort =
            [
                new()
                {
                    Property = propertyName,
                    Descending = descending
                }
            ]
        };

        QueryBuildOptions<Goblin> options = new()
        {
            StringComparisonType = comparisonType,
        };
        
        // Act
        
        IQueryable<Goblin> query = fixture.SampleData.ApplyQuery(request, options);
        
        // Assert
        
        Assert.SkipWhen(TestDataFixture.Provider != DataProvider.Runtime,
            "Order by clauses with StringComparer are not available in SQL EF Core");
        
        Assert.True(query.AsEnumerable().ElementAt(13).Id == expectedId);
    }

    [Fact]
    public void ApplyQuery_ExcludedPropertyOption_ShouldThrowOnExcludedProperty()
    {
        
        // Arrange
        
        QueryRequest request = new()
        {
            Sort =
            [
                new()
                {
                    Property = "Name",
                }
            ]
        };

        QueryBuildOptions<Goblin> options = new()
        {
            ExcludedProperties = [x=>x.Name]
        };
        
        // Act

        try
        {
            fixture.SampleData.ApplyQuery(request, options);
        }
        catch (QueryBuildException ex)
        {
            //Assert
            Assert.True(ex.ExceptionType == QueryBuildExceptionType.MissingProperty);
            return;
        }
        
        Assert.Fail();
    }

    [Fact]
    public void ApplyQuery_IncludedPropertyOption_ShouldThrowOnNotIncludedProperty()
    {
        
        // Arrange
        
        QueryRequest request = new()
        {
            Sort =
            [
                new()
                {
                    Property = "Name",
                }
            ]
        };

        QueryBuildOptions<Goblin> options = new()
        {
            IncludedProperties = [x=>x.Id]
        };
        
        // Act

        try
        {
            fixture.SampleData.ApplyQuery(request, options);
        }
        catch (QueryBuildException ex)
        {
            //Assert
            Assert.True(ex.ExceptionType == QueryBuildExceptionType.MissingProperty);
            return;
        }
        
        Assert.Fail();
    }

    [Theory]
    [InlineData(DateTimeKind.Utc)]
    [InlineData(DateTimeKind.Unspecified)]
    public void ApplyQuery_OverrideDateTimeKindOption_ShouldReturnCountOnFilter(DateTimeKind? dateTimeKind)
    {
        
        // Arrange
        
        QueryRequest request = new()
        {
            Filters =
            [
                new()
                {
                    Property = "DateOfBirth",
                    Value = "2022-06-15T12:34:56Z"
                }
            ]
        };

        QueryBuildOptions<Goblin> options = new()
        {
            OverrideDateTimeKind = dateTimeKind,
        };
        
        // Act
        
        IQueryable<Goblin> query = fixture.SampleData.ApplyQuery(request, options);
        
        // Assert

        DateTime? queryDateTime =
            ((ConstantExpression)
                ((BinaryExpression)
                    ((LambdaExpression)
                        ((UnaryExpression)
                            ((MethodCallExpression)query.Expression).Arguments[1]).Operand).Body).Right).Value as DateTime?;
        
        Assert.NotNull(queryDateTime);
        
        Assert.True(queryDateTime.Value.Kind == dateTimeKind);
    }

    [Fact]
    public void ApplyQuery_PropertyMappingOption_ShouldReturnIdOnFilterAndSort()
    {
        
        // Arrange
        
        QueryRequest request = new()
        {
            Filters =
            [
                new()
                {
                    Property = "DOB",
                    Value = "2022-06-15T12:34:56Z",
                    Operator = FilterOperator.LessThan
                },
                new()
                {
                    Property = "GoblinName",
                    Operator = FilterOperator.IsNotEmpty
                }
            ],
            Sort = [
                new()
                {
                    Property = "GoblinId",
                    Descending = true
                }
            ]
        };

        QueryBuildOptions<Goblin> options = new()
        {
            PropertyMapping = new()
            {
                {"DOB", x=>x.DateOfBirth},
                {"GoblinName", x=>x.Name},
                {"GoblinId", x=>x.Id}
            }
        };
        
        // Act
        
        IQueryable<Goblin> query = fixture.SampleData.ApplyQuery(request, options);
        
        // Assert
        
        Assert.True(query.First().Id == 30);
    }

    [Fact]
    public void ApplyQuery_HideMappedPropertiesOption_ShouldThrowOnDirectAccessOfMappedProperty()
    {
        
        // Arrange
        
        QueryRequest request = new()
        {
            Filters =
            [
                new()
                {
                    Property = "DateOfBirth",
                    Value = "2022-06-15T12:34:56Z",
                    Operator = FilterOperator.LessThan
                }
            ]
        };

        QueryBuildOptions<Goblin> options = new()
        {
            PropertyMapping = new()
            {
                {"DOB", x=>x.DateOfBirth}
            }
        };
        
        //Act

        try
        {
            fixture.SampleData.ApplyQuery(request, options);
        }
        catch (QueryBuildException ex)
        {
            //Assert
            Assert.True(ex.ExceptionType == QueryBuildExceptionType.MissingProperty);
            return;
        }
        
        Assert.Fail();
    }

    [Fact]
    public void ApplyQuery_HideMappedPropertiesOption_ShouldNotThrowOnDirectAccessOfMappedProperty()
    {
        
        // Arrange
        
        QueryRequest request = new()
        {
            Filters =
            [
                new()
                {
                    Property = "DateOfBirth",
                    Value = "2022-06-15T12:34:56Z",
                    Operator = FilterOperator.LessThan
                }
            ]
        };

        QueryBuildOptions<Goblin> options = new()
        {
            PropertyMapping = new()
            {
                {"DOB", x=>x.DateOfBirth}
            },
            HideMappedProperties = false
        };
        
        //Act
        
        fixture.SampleData.ApplyQuery(request, options);
        
        Assert.True(true);
    }

    [Fact]
    public void ApplyQuery_BindingFlagsOption_ShouldThrow()
    {
        
        // Arrange
        
        QueryRequest request = new()
        {
            Sort =
            [
                new()
                {
                    Property = "Name",
                }
            ]
        };

        QueryBuildOptions<Goblin> options = new()
        {
            PropertyBindingFlags = BindingFlags.ExactBinding
        };
        
        // Act

        try
        {
            fixture.SampleData.ApplyQuery(request, options);
        }
        catch (QueryBuildException ex)
        {
            //Assert
            Assert.True(ex.ExceptionType == QueryBuildExceptionType.MissingProperty);
            return;
        }
        
        Assert.Fail();
    }
}