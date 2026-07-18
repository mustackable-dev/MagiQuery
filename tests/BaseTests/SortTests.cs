using MagiQuery.Extensions;
using MagiQuery.Models;
using MagiQueryTests.Entities;
using MagiQueryTests.Fixtures;

namespace MagiQueryTests.BaseTests;

public abstract class SortTests
{
    protected abstract ITestDataFixture Fixture { get; }
    
    [Fact]
    public void ApplyQuery_NoSort_ShouldNotSortList()
    {
        // Arrange

        QueryRequest request = new();
        
        // Act
        
        IEnumerable<Goblin> query = Fixture.SampleData.ApplyQuery(request).ToArray();
        
        // Assert
        
        Assert.True(query.First().Id == 1);
        Assert.True(query.Last().Id == 30);
    }
    
    [Theory]
    [InlineData("Id", 1)]
    [InlineData("Name", 27)]
    [InlineData("FavouriteLetter", 18, 4)]
    [InlineData("IntelligenceLevel", 22)]
    [InlineData("Age", 23)]
    [InlineData("PowerLevel", 3)]
    [InlineData("Stamina", 10)]
    [InlineData("ExperiencePoints", 1)]
    [InlineData("MagicPower", 1)]
    [InlineData("Mana", 9)]
    [InlineData("Strength", 16)]
    [InlineData("Agility", 27)]
    [InlineData("Salary", 1)]
    [InlineData("IsActive", 2)]
    [InlineData("Taste", 8)]
    [InlineData("DateOfBirth", 22)]
    [InlineData("DateOfConception", 22)]
    [InlineData("HobbitAncestry", 1)]
    public void ApplyQuery_SingleSortAscending_ShouldSortList(
        string propertyName,
        int expectedId,
        int expectedIdCaseInsensitive = -1)
    {
        // Arrange
        
        QueryRequest request = new()
        {
            Sorts = [new()
            {
                Property = propertyName,
            }]
        };
        
        // Act
        
        IQueryable<Goblin> query = Fixture.SampleData.ApplyQuery(request);
        
        // Assert

        bool caseInsensitiveCheck = 
            expectedIdCaseInsensitive != -1 &&
            Array.Exists([
                DataProvider.SqlServer,
                DataProvider.MySqlPomelo,
                DataProvider.MySql
            ], x=>x==Fixture.Provider);
        
        Assert.SkipWhen(
            Fixture.Provider==DataProvider.Sqlite && propertyName == "DateOfConception",
            "Sorting is not supported for type 'DateTimeOffset' in Sqlite");
        
        Assert.SkipWhen(
            Fixture.Provider==DataProvider.Sqlite && propertyName == "Mana",
            "Sorting is not supported for type 'ulong' in Sqlite");
        
        Assert.SkipWhen(
            Fixture.Provider==DataProvider.Sqlite && propertyName == "Salary",
            "Sorting is not supported for type 'decimal' in Sqlite");

        if (Fixture.Provider == DataProvider.SqlServer && propertyName == "HobbitAncestry")
        {
            //SQL Server's TOP(1) query translation of First() for int version of HobbitAncestry
            //in ascending order returns Id=2 instead of Id=1, probably because of the way
            //Clustered indices work, so we are using ElementAt(0) in this specific instance.

            Assert.True(query.ElementAt(0).Id == (!caseInsensitiveCheck ? expectedId : expectedIdCaseInsensitive));
            return;
        }
        
        Assert.True(query.First().Id == (!caseInsensitiveCheck ? expectedId : expectedIdCaseInsensitive));
    }
    
    [Theory]
    [InlineData("Id", 30)]
    [InlineData("Name", 29)]
    [InlineData("FavouriteLetter", 3)]
    [InlineData("IntelligenceLevel", 17)]
    [InlineData("Age", 5)]
    [InlineData("PowerLevel", 20)]
    [InlineData("Stamina", 23)]
    [InlineData("ExperiencePoints", 24)]
    [InlineData("MagicPower", 16)]
    [InlineData("Mana", 16)]
    [InlineData("Strength", 22)]
    [InlineData("Agility", 22)]
    [InlineData("Salary", 21)]
    [InlineData("IsActive", 1)]
    [InlineData("Taste", 22)]
    [InlineData("DateOfBirth", 1)]
    [InlineData("DateOfConception", 1)]
    [InlineData("HobbitAncestry", 22)]
    public void ApplyQuery_SingleSortDescending_ShouldSortList(
        string propertyName,
        int expectedId)
    {
        // Arrange
        
        QueryRequest request = new()
        {
            Sorts = [new()
            {
                Property = propertyName,
                Descending = true
            }]
        };
        
        // Act
        
        IQueryable<Goblin> query = Fixture.SampleData.ApplyQuery(request);
        
        // Assert
        
        Assert.SkipWhen(
            Fixture.Provider==DataProvider.Sqlite && propertyName == "DateOfConception",
            "Sorting is not supported for type 'DateTimeOffset' in Sqlite");
        
        Assert.SkipWhen(
            Fixture.Provider==DataProvider.Sqlite && propertyName == "Mana",
            "Sorting is not supported for type 'ulong' in Sqlite");
        
        Assert.SkipWhen(
            Fixture.Provider==DataProvider.Sqlite && propertyName == "Salary",
            "Sorting is not supported for type 'decimal' in Sqlite");
        
        Assert.True(query.First().Id == expectedId);
    }
    
    [Fact]
    public void ApplyQuery_MultipleSortsAscending_ShouldSortList()
    {
        // Arrange
        
        QueryRequest request = new()
        {
            Filters = [new()
            {
                Property = "MagicPower",
                Value = "1234567890",
                Operator = FilterOperator.Equals
            }],
            Sorts = [new()
            {
                Property = "DateOfBirth",
            },new()
            {
                Property = "Stamina",
            }]
        };
        
        // Act
        
        IQueryable<Goblin> query = Fixture.SampleData.ApplyQuery(request);
        
        // Assert

        Assert.True(query.First().Id == 25);
    }
    
    [Fact]
    public void ApplyQuery_MultipleSortsDescending_ShouldSortList()
    {
        // Arrange
        
        QueryRequest request = new()
        {
            Filters = [new()
            {
                Property = "MagicPower",
                Value = "1234567890",
                Operator = FilterOperator.Equals
            }],
            Sorts = [new()
            {
                Property = "Age",
                Descending = true
            },new()
            {
                Property = "Stamina",
                Descending = true
            }]
        };
        
        // Act
        
        IQueryable<Goblin> query = Fixture.SampleData.ApplyQuery(request);
        
        // Assert
        
        Assert.True(query.First().Id == 9);
    }
    
    [Fact]
    public void ApplyQuery_MultipleSortsMixedOrder_ShouldSortList()
    {
        // Arrange
        
        QueryRequest request = new()
        {
            Filters = [new()
            {
                Property = "MagicPower",
                Value = "1234567890",
                Operator = FilterOperator.Equals
            }],
            Sorts = [new()
            {
                Property = "DateOfBirth",
                Descending = true
            },new()
            {
                Property = "Stamina"
            }]
        };
        
        // Act
        
        IQueryable<Goblin> query = Fixture.SampleData.ApplyQuery(request);
        
        // Assert
        
        Assert.True(query.Skip(2).First().Id == 17);
    }
}