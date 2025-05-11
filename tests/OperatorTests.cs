using MagiQueryTests.Fixtures;
using MagiQueryTests.Entities;
using MagiQuery.Models;
using MagiQuery.Extensions;

namespace MagiQueryTests;

[Collection("SampleDataCollection")]
public class OperatorTests(TestDataFixture fixture)
{
    [Theory]
    [InlineData("Id", "22")]
    [InlineData("Name", "Glimglo")]
    [InlineData("FavouriteLetter", "j")]
    [InlineData("IntelligenceLevel", "-15")]
    [InlineData("Age", "60")]
    [InlineData("PowerLevel", "2345")]
    [InlineData("Stamina", "54")]
    [InlineData("ExperiencePoints", "7090123")]
    [InlineData("MagicPower", "6789012346")]
    [InlineData("Mana", "7891234567")]
    [InlineData("Strength", "36.1")]
    [InlineData("Agility", "36.0")]
    [InlineData("Salary", "55000")]
    [InlineData("HobbitAncestry", "true")]
    [InlineData("Taste", "Tangy")]
    [InlineData("DateOfBirth", "1020-03-14T00:00:00")]
    [InlineData("DateOfConception", "1019-02-24T19:50:15+07:00")]
    [InlineData("Contract.SigningDate", "1191-01-02")]
    [InlineData("Contract.Details.SigningTime", "00:03:00")]
    [InlineData("Contract.Details.Duration", "02:00:00")]
    public void ApplyQuery_EqualsOperator_ShouldReturnEntry(string property, string value)
    {
        // Arrange
        
        QueryRequest request = new()
        {
            Filters =
            [
                new()
                {
                    Property = property,
                    Value = value,
                    Operator = FilterOperator.Equals
                }
            ]
        };

        QueryBuildOptions<Goblin> options = new()
        {
            OverrideDateTimeKind = TestDataFixture.Provider == DataProvider.MongoDb ? DateTimeKind.Utc : null
        };
        
        // Act

        #region MongoDbSpecific
        
        // MongoDb branch is necessary here, since the provider currently does not support
        // evaluation of nested nullable properties
        
        if(TestDataFixture.Provider == DataProvider.MongoDb && property.StartsWith("Contract."))
        {
            int evaluationCount = 0;
            switch(property)
            {
                case "Contract.Details.Duration":
                    request.Filters.First().Property = "Duration";
                    evaluationCount = fixture.ContractDetails.ApplyQuery(request).Count();
                    break;
                case "Contract.Details.SigningTime":
                    Assert.SkipWhen(true, "MongoDb stores TimeOnly as integer, string comparisons will not work");
                    return;
                case "Contract.SigningDate":
                    request.Filters.First().Property = "SigningDate";
                    evaluationCount = fixture.Contracts.ApplyQuery(request).Count();
                    break;
            }
            
            //Assert
            
            Assert.True(evaluationCount == 1);
            
            return;
        }

        #endregion
        
        IQueryable<Goblin> query = fixture.SampleData.ApplyQuery(request, options);
        
        // Assert
        
        Assert.SkipWhen(
            property == "Strength" && TestDataFixture.Provider != DataProvider.Runtime,
            "We skip this equality test for the float type on data sources other than Runtime");
        
        Assert.True(query.Count() == 1);
        Assert.True(query.First().Id == 22);
    }
    
    [Theory]
    [InlineData("Id", "22")]
    [InlineData("Name", "Glimglo")]
    [InlineData("FavouriteLetter", "j")]
    [InlineData("IntelligenceLevel", "-15")]
    [InlineData("Age", "60")]
    [InlineData("PowerLevel", "2345")]
    [InlineData("Stamina", "54")]
    [InlineData("ExperiencePoints", "7090123")]
    [InlineData("MagicPower", "6789012346")]
    [InlineData("Mana", "7891234567")]
    [InlineData("Strength", "36.1")]
    [InlineData("Agility", "36.0")]
    [InlineData("Salary", "55000")]
    [InlineData("HobbitAncestry", "true")]
    [InlineData("Taste", "Tangy")]
    [InlineData("DateOfBirth", "1020-03-14T00:00:00")]
    [InlineData("DateOfConception", "1019-02-24T19:50:15+07:00")]
    [InlineData("Contract.SigningDate", "1191-01-02")]
    [InlineData("Contract.Details.SigningTime", "00:03:00")]
    [InlineData("Contract.Details.Duration", "02:00:00")]
    public void ApplyQuery_DoesNotEqualOperator_ShouldNotReturnEntry(string property, string value)
    {
        // Arrange
        
        QueryRequest request = new()
        {
            Filters =
            [
                new()
                {
                    Property = property,
                    Value = value,
                    Operator = FilterOperator.DoesNotEqual
                }
            ]
        };

        QueryBuildOptions<Goblin> options = new()
        {
            OverrideDateTimeKind = TestDataFixture.Provider == DataProvider.MongoDb ? DateTimeKind.Utc : null
        };
        
        // Act

        #region MongoDbSpecific
        
        // MongoDb branch is necessary here, since the provider currently does not support
        // evaluation of nested nullable properties
        
        if(TestDataFixture.Provider == DataProvider.MongoDb && property.StartsWith("Contract."))
        {
            int evaluationCount = 0;
            switch(property)
            {
                case "Contract.Details.Duration":
                    request.Filters.First().Property = "Duration";
                    evaluationCount = fixture.ContractDetails.ApplyQuery(request).Count();
                    break;
                case "Contract.Details.SigningTime":
                    Assert.SkipWhen(true, "MongoDb stores TimeOnly as integer, string comparisons will not work");
                    return;
                case "Contract.SigningDate":
                    request.Filters.First().Property = "SigningDate";
                    evaluationCount = fixture.Contracts.ApplyQuery(request).Count();
                    break;
            }
            
            //Assert
            
            Assert.True(evaluationCount > 1);
            
            return;
        }

        #endregion
        
        IQueryable<Goblin> query = fixture.SampleData.ApplyQuery(request, options);
        
        // Assert
        
        Assert.SkipWhen(
            property == "Strength" && TestDataFixture.Provider != DataProvider.Runtime,
            "We skip this equality test for the float type on data sources other than Runtime");
        
        Assert.True(query.Count() == 29);
        Assert.False(query.Any(x=> x.Id == 22));
    }
    
    [Theory]
    [InlineData("Id", "22", 8)]
    [InlineData("IntelligenceLevel", "-15", 29)]
    [InlineData("Age", "60", 15)]
    [InlineData("PowerLevel", "2345", 12)]
    [InlineData("Stamina", "54", 14)]
    [InlineData("ExperiencePoints", "7090123", 2)]
    [InlineData("MagicPower", "6789012346", 7)]
    [InlineData("Mana", "7891234567", 6)]
    [InlineData("Strength", "36.1", 0)]
    [InlineData("Agility", "36.0", 0)]
    [InlineData("Salary", "55000", 8)]
    [InlineData("Taste", "3", 14)]
    [InlineData("DateOfBirth", "1020-03-14T00:00:00", 29)]
    [InlineData("DateOfConception", "1019-02-24T19:50:15+07:00", 29)]
    [InlineData("Contract.SigningDate", "1192-01-01", 2)]
    [InlineData("Contract.Details.SigningTime", "00:01:00", 2)]
    [InlineData("Contract.Details.Duration", "00:00:36", 1)]
    public void ApplyQuery_GreaterThanOperator_ShouldReturnEntries(string property, string value, int expectedCount)
    {
        // Arrange
        
        QueryRequest request = new()
        {
            Filters =
            [
                new()
                {
                    Property = property,
                    Value = value,
                    Operator = FilterOperator.GreaterThan
                }
            ]
        };

        QueryBuildOptions<Goblin> options = new()
        {
            OverrideDateTimeKind = TestDataFixture.Provider == DataProvider.MongoDb ? DateTimeKind.Utc : null
        };
        
        // Act

        #region MongoDbSpecific
        
        // MongoDb branch is necessary here, since the provider currently does not support
        // evaluation of nested nullable properties
        
        if(TestDataFixture.Provider == DataProvider.MongoDb && property.StartsWith("Contract."))
        {
            int evaluationCount = 0;
            switch(property)
            {
                case "Contract.Details.Duration":
                    request.Filters.First().Property = "Duration";
                    evaluationCount = fixture.ContractDetails.ApplyQuery(request).Count();
                    break;
                case "Contract.Details.SigningTime":
                    Assert.SkipWhen(true, "MongoDb stores TimeOnly as integer, string comparisons will not work");
                    return;
                case "Contract.SigningDate":
                    request.Filters.First().Property = "SigningDate";
                    evaluationCount = fixture.Contracts.ApplyQuery(request).Count();
                    break;
            }
            
            //Assert
            
            Assert.True(evaluationCount == expectedCount);
            
            return;
        }

        #endregion
        
        IQueryable<Goblin> query = fixture.SampleData.ApplyQuery(request, options);
        
        // Assert
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.Sqlite && property == "Mana",
            "Operator not supported for type 'ulong' in Sqlite");
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.Sqlite && property == "Contract.Details.Duration",
            "Operator not supported for type 'TimeSpan' in Sqlite");
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.Sqlite && property == "DateOfConception",
            "Operator not supported for type 'DateTimeOffset' in Sqlite");
        
        Assert.True(query.Count() == expectedCount);
    }
    
    [Theory]
    [InlineData("Id", "22", 9)]
    [InlineData("IntelligenceLevel", "-15", 30)]
    [InlineData("Age", "60", 16)]
    [InlineData("PowerLevel", "2345", 13)]
    [InlineData("Stamina", "54", 15)]
    [InlineData("ExperiencePoints", "7090123", 3)]
    [InlineData("MagicPower", "6789012346", 8)]
    [InlineData("Mana", "7891234567", 7)]
    [InlineData("Strength", "36.09", 1)]
    [InlineData("Agility", "36.0", 1)]
    [InlineData("Salary", "55000", 9)]
    [InlineData("Taste", "3", 18)]
    [InlineData("DateOfBirth", "1020-03-14T00:00:00", 30)]
    [InlineData("DateOfConception", "1019-02-24T19:50:15+07:00", 30)]
    [InlineData("Contract.SigningDate", "1192-01-02", 2)]
    [InlineData("Contract.Details.SigningTime", "00:01:00", 2)]
    [InlineData("Contract.Details.Duration", "00:00:36", 2)]
    public void ApplyQuery_GreaterThanOrEqualOperator_ShouldReturnEntries(string property, string value, int expectedCount)
    {
        // Arrange
        
        QueryRequest request = new()
        {
            Filters =
            [
                new()
                {
                    Property = property,
                    Value = value,
                    Operator = FilterOperator.GreaterThanOrEqual
                }
            ]
        };

        QueryBuildOptions<Goblin> options = new()
        {
            OverrideDateTimeKind = TestDataFixture.Provider == DataProvider.MongoDb ? DateTimeKind.Utc : null
        };
        
        // Act

        #region MongoDbSpecific
        
        // MongoDb branch is necessary here, since the provider currently does not support
        // evaluation of nested nullable properties
        
        if(TestDataFixture.Provider == DataProvider.MongoDb && property.StartsWith("Contract."))
        {
            int evaluationCount = 0;
            switch(property)
            {
                case "Contract.Details.Duration":
                    request.Filters.First().Property = "Duration";
                    evaluationCount = fixture.ContractDetails.ApplyQuery(request).Count();
                    break;
                case "Contract.Details.SigningTime":
                    Assert.SkipWhen(true, "MongoDb stores TimeOnly as integer, string comparisons will not work");
                    return;
                case "Contract.SigningDate":
                    request.Filters.First().Property = "SigningDate";
                    evaluationCount = fixture.Contracts.ApplyQuery(request).Count();
                    break;
            }
            
            //Assert
            
            Assert.True(evaluationCount == expectedCount);
            
            return;
        }

        #endregion
        
        IQueryable<Goblin> query = fixture.SampleData.ApplyQuery(request, options);
        
        // Assert
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.Sqlite && property == "Mana",
            "Operator not supported for type 'ulong' in Sqlite");
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.Sqlite && property == "Contract.Details.Duration",
            "Operator not supported for type 'TimeSpan' in Sqlite");
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.Sqlite && property == "DateOfConception",
            "Operator not supported for type 'DateTimeOffset' in Sqlite");
        
        Assert.True(query.Count() == expectedCount);
    }
    
    [Theory]
    [InlineData("Id", "22", 21)]
    [InlineData("IntelligenceLevel", "-15", 0)]
    [InlineData("Age", "60", 14)]
    [InlineData("PowerLevel", "2345", 17)]
    [InlineData("Stamina", "54", 15)]
    [InlineData("ExperiencePoints", "7090123", 27)]
    [InlineData("MagicPower", "6789012346", 22)]
    [InlineData("Mana", "7891234567", 23)]
    [InlineData("Strength", "36.09", 29)]
    [InlineData("Agility", "36.0", 29)]
    [InlineData("Salary", "55000", 21)]
    [InlineData("Taste", "3", 12)]
    [InlineData("DateOfBirth", "1020-03-14T00:00:00", 0)]
    [InlineData("DateOfConception", "1019-02-24T19:50:15+07:00", 0)]
    [InlineData("Contract.SigningDate", "2025-01-01", 2)]
    [InlineData("Contract.Details.SigningTime", "01:00:00", 2)]
    [InlineData("Contract.Details.Duration", "00:00:36", 1)]
    public void ApplyQuery_LessThanOperator_ShouldReturnEntries(string property, string value, int expectedCount)
    {
        // Arrange
        
        QueryRequest request = new()
        {
            Filters =
            [
                new()
                {
                    Property = property,
                    Value = value,
                    Operator = FilterOperator.LessThan
                }
            ]
        };

        QueryBuildOptions<Goblin> options = new()
        {
            OverrideDateTimeKind = TestDataFixture.Provider == DataProvider.MongoDb ? DateTimeKind.Utc : null
        };
        
        // Act

        #region MongoDbSpecific
        
        // MongoDb branch is necessary here, since the provider currently does not support
        // evaluation of nested nullable properties
        
        if(TestDataFixture.Provider == DataProvider.MongoDb && property.StartsWith("Contract."))
        {
            int evaluationCount = 0;
            switch(property)
            {
                case "Contract.Details.Duration":
                    request.Filters.First().Property = "Duration";
                    evaluationCount = fixture.ContractDetails.ApplyQuery(request).Count();
                    break;
                case "Contract.Details.SigningTime":
                    Assert.SkipWhen(true, "MongoDb stores TimeOnly as integer, string comparisons will not work");
                    return;
                case "Contract.SigningDate":
                    request.Filters.First().Property = "SigningDate";
                    evaluationCount = fixture.Contracts.ApplyQuery(request).Count();
                    break;
            }
            
            //Assert
            
            Assert.True(evaluationCount == expectedCount);
            
            return;
        }

        #endregion
        
        IQueryable<Goblin> query = fixture.SampleData.ApplyQuery(request, options);
        
        // Assert
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.Sqlite && property == "Mana",
            "Operator not supported for type 'ulong' in Sqlite");
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.Sqlite && property == "Contract.Details.Duration",
            "Operator not supported for type 'TimeSpan' in Sqlite");
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.Sqlite && property == "DateOfConception",
            "Operator not supported for type 'DateTimeOffset' in Sqlite");
        
        Assert.True(query.Count() == expectedCount);
    }
    
    [Theory]
    [InlineData("Id", "22", 22)]
    [InlineData("IntelligenceLevel", "-15", 1)]
    [InlineData("Age", "60", 15)]
    [InlineData("PowerLevel", "2345", 18)]
    [InlineData("Stamina", "54", 16)]
    [InlineData("ExperiencePoints", "7090123", 28)]
    [InlineData("MagicPower", "6789012346", 23)]
    [InlineData("Mana", "7891234567", 24)]
    [InlineData("Strength", "36.1", 30)]
    [InlineData("Agility", "36.0", 30)]
    [InlineData("Salary", "55000", 22)]
    [InlineData("Taste", "3", 16)]
    [InlineData("DateOfBirth", "1020-03-14T00:00:00", 1)]
    [InlineData("DateOfConception", "1019-02-24T19:50:15+07:00", 1)]
    [InlineData("Contract.SigningDate", "2025-01-01", 3)]
    [InlineData("Contract.Details.SigningTime", "01:00:00", 3)]
    [InlineData("Contract.Details.Duration", "00:00:36", 2)]
    public void ApplyQuery_LessThanOrEqualOperator_ShouldReturnEntries(string property, string value, int expectedCount)
    {
        // Arrange
        
        QueryRequest request = new()
        {
            Filters =
            [
                new()
                {
                    Property = property,
                    Value = value,
                    Operator = FilterOperator.LessThanOrEqual
                }
            ]
        };

        QueryBuildOptions<Goblin> options = new()
        {
            OverrideDateTimeKind = TestDataFixture.Provider == DataProvider.MongoDb ? DateTimeKind.Utc : null
        };
        
        // Act

        #region MongoDbSpecific
        
        // MongoDb branch is necessary here, since the provider currently does not support
        // evaluation of nested nullable properties
        
        if(TestDataFixture.Provider == DataProvider.MongoDb && property.StartsWith("Contract."))
        {
            int evaluationCount = 0;
            switch(property)
            {
                case "Contract.Details.Duration":
                    request.Filters.First().Property = "Duration";
                    evaluationCount = fixture.ContractDetails.ApplyQuery(request).Count();
                    break;
                case "Contract.Details.SigningTime":
                    Assert.SkipWhen(true, "MongoDb stores TimeOnly as integer, string comparisons will not work");
                    return;
                case "Contract.SigningDate":
                    request.Filters.First().Property = "SigningDate";
                    evaluationCount = fixture.Contracts.ApplyQuery(request).Count();
                    break;
            }
            
            //Assert
            
            Assert.True(evaluationCount == expectedCount);
            
            return;
        }

        #endregion
        
        IQueryable<Goblin> query = fixture.SampleData.ApplyQuery(request, options);
        
        // Assert
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.Sqlite && property == "Mana",
            "Operator not supported for type 'ulong' in Sqlite");
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.Sqlite && property == "Contract.Details.Duration",
            "Operator not supported for type 'TimeSpan' in Sqlite");
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.Sqlite && property == "DateOfConception",
            "Operator not supported for type 'DateTimeOffset' in Sqlite");
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.Sqlite && property == "Mana",
            "Operator not supported for type 'ulong' in Sqlite");
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.Sqlite && property == "Contract.Details.Duration",
            "Operator not supported for type 'TimeSpan' in Sqlite");
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.Sqlite && property == "DateOfConception",
            "Operator not supported for type 'DateTimeOffset' in Sqlite");
        
        Assert.True(query.Count() == expectedCount);
    }
    
    [Theory]
    [InlineData("Id", "22")]
    [InlineData("Name", "Glimgl")]
    [InlineData("FavouriteLetter", "j")]
    [InlineData("IntelligenceLevel", "-15")]
    [InlineData("Age", "60")]
    [InlineData("PowerLevel", "2345")]
    [InlineData("Stamina", "54")]
    [InlineData("ExperiencePoints", "709012")]
    [InlineData("MagicPower", "67890123", 4, 5)]
    [InlineData("Mana", "789123456")]
    [InlineData("Strength", "36.")]
    [InlineData("Agility", "3")]
    [InlineData("Salary", "5500")]
    [InlineData("HobbitAncestry", "Tru")]
    [InlineData("Taste", "Tang")]
    [InlineData("DateOfBirth", "1020-03-14", 1, 22, "s")]
    [InlineData("DateOfConception", "1019-02-24", 1, 22, "s")]
    [InlineData("Contract.SigningDate", "2025", 1, 20, "yyyy MMMM dd")]
    [InlineData("Contract.Details.SigningTime", "01", 1, 21)]
    [InlineData("Contract.Details.Duration", "00", 2, 20)]
    public void ApplyQuery_StartsWithOperator_ShouldReturnEntries(
        string property,
        string value,
        int expectedCount = 1,
        int expectedFirst = 22,
        string? exactParseFormat = null,
        string? overrideCulture = null)
    {
        // Arrange
        
        QueryRequest request = new()
        {
            Filters =
            [
                new()
                {
                    Property = property,
                    Value = value,
                    Operator = FilterOperator.StartsWith,
                    ExactParseFormat = exactParseFormat,
                    OverrideCulture = overrideCulture
                }
            ]
        };
        
        // Act

        #region MongoDbSpecific
        
        // MongoDb branch is necessary here, since the provider currently does not support
        // evaluation of nested nullable properties
        
        if(TestDataFixture.Provider == DataProvider.MongoDb && property.StartsWith("Contract."))
        {
            int evaluationCount = 0;
            switch(property)
            {
                case "Contract.Details.Duration":
                    request.Filters.First().Property = "Duration";
                    evaluationCount = fixture.ContractDetails.ApplyQuery(request).Count();
                    break;
                case "Contract.Details.SigningTime":
                    Assert.SkipWhen(true, "MongoDb stores TimeOnly as integer, string comparisons will not work");
                    return;
                case "Contract.SigningDate":
                    request.Filters.First().Property = "SigningDate";
                    evaluationCount = fixture.Contracts.ApplyQuery(request).Count();
                    break;
            }
            
            //Assert
            
            Assert.True(evaluationCount == expectedCount);
            
            return;
        }

        #endregion
        
        IQueryable<Goblin> query = fixture.SampleData.ApplyQuery(request);
        
        // Assert
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.Sqlite && property == "Mana",
            "Operator not supported for type 'ulong' in Sqlite");
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.Sqlite && property == "Contract.Details.Duration",
            "Operator not supported for type 'TimeSpan' in Sqlite");
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.Sqlite && property == "DateOfConception",
            "Operator not supported for type 'DateTimeOffset' in Sqlite");
        
        Assert.SkipWhen(
            TestDataFixture.Provider == DataProvider.MySql,
            "\"StartsWith\" is not supported with this provider.");
        
        Assert.SkipWhen(
            Array.Exists([DataProvider.PostgreSql, DataProvider.MySqlPomelo], x=>x == TestDataFixture.Provider) &&
            Array.Exists(["Contract.Details.SigningTime", "Contract.SigningDate"], x=>x == property),
            "The DateOnly and TimeOnly string comparison is not supported with this provider.");
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.Sqlite && property == "Mana",
            "This operator is not supported for type 'ulong' in Sqlite");
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.MongoDb && property == "DateOfConception",
            "Limited support for comparison on string conversions of DateTimeOffset in MongoDB");
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.MongoDb && property == "FavouriteLetter",
            "MongoDB stores char as number by default with MongoDB.EntityFrameworkCore");
        
        Assert.True(query.Count() == expectedCount);
        Assert.True(query.First().Id == expectedFirst);
    }
    
    [Theory]
    [InlineData("Id", "2", 3, 2)]
    [InlineData("Name", "imglo")]
    [InlineData("FavouriteLetter", "j")]
    [InlineData("IntelligenceLevel", "5", 3, 14)]
    [InlineData("Age", "0", 7, 1)]
    [InlineData("PowerLevel", "5", 3, 1)]
    [InlineData("Stamina", "4", 3, 10)]
    [InlineData("ExperiencePoints", "12", 1, 8)]
    [InlineData("MagicPower", "123", 4, 3)]
    [InlineData("Mana", "456", 2, 21)]
    [InlineData("Strength", ".10", 4, 3, "0.00")]
    [InlineData("Agility", ".80", 1, 11, "0.00")]
    [InlineData("Salary", "0", 15, 16)]
    [InlineData("HobbitAncestry", "e", 30, 1)]
    [InlineData("Taste", "itter", 4, 5)]
    [InlineData("DateOfBirth", "00", 15, 16, "s")]
    [InlineData("DateOfConception", "00", 30, 1, "yyyy-MM-ddTHH:mm:sszzz")]
    [InlineData("Contract.SigningDate", "01", 1, 20, "yyyy MMMM dd")]
    [InlineData("Contract.Details.SigningTime", "00", 3, 20, "HH:mm:ss")]
    [InlineData("Contract.Details.Duration", "10", 1, 21, "g")]
    public void ApplyQuery_EndsWithOperator_ShouldReturnEntries(
        string property,
        string value,
        int expectedCount = 1,
        int expectedFirst = 22,
        string? exactParseFormat = null,
        string? overrideCulture = null)
    {
        // Arrange
        
        QueryRequest request = new()
        {
            Filters =
            [
                new()
                {
                    Property = property,
                    Value = value,
                    Operator = FilterOperator.EndsWith,
                    ExactParseFormat = exactParseFormat,
                    OverrideCulture = overrideCulture
                }
            ]
        };
        
        // Act

        #region MongoDbSpecific
        
        // MongoDb branch is necessary here, since the provider currently does not support
        // evaluation of nested nullable properties
        
        if(TestDataFixture.Provider == DataProvider.MongoDb && property.StartsWith("Contract."))
        {
            int evaluationCount = 0;
            switch(property)
            {
                case "Contract.Details.Duration":
                    request.Filters.First().Property = "Duration";
                    evaluationCount = fixture.ContractDetails.ApplyQuery(request).Count();
                    break;
                case "Contract.Details.SigningTime":
                    Assert.SkipWhen(true, "MongoDb stores TimeOnly as integer, string comparisons will not work");
                    return;
                case "Contract.SigningDate":
                    Assert.SkipWhen(true, "MongoDb stores DateOnly as DateTime, this expression will always end " +
                                          "in 00.");
                    return;
            }
            
            //Assert
            
            Assert.True(evaluationCount == expectedCount);
            
            return;
        }

        #endregion
        
        IQueryable<Goblin> query = fixture.SampleData.ApplyQuery(request);
        
        // Assert
        
        Assert.SkipWhen(
            TestDataFixture.Provider == DataProvider.MySql,
            "\"EndsWith\" is not supported with this provider.");
        
        Assert.SkipWhen(
            TestDataFixture.Provider != DataProvider.Runtime &&
            (property == "Strength" || property == "Agility"),
            "Skipping due to differences in rounding between data sources");
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.Sqlite && property == "Mana",
            "This operator is not supported for type 'ulong' in Sqlite");
        
        Assert.SkipWhen(
            Array.Exists([DataProvider.PostgreSql, DataProvider.MySqlPomelo], x=>x == TestDataFixture.Provider) &&
            Array.Exists(["Contract.Details.SigningTime", "Contract.SigningDate"], x=>x == property),
            "The DateOnly and TimeOnly string comparison is not supported with this provider.");
        
        Assert.SkipWhen(
            Array.Exists(
                [DataProvider.SqlServer, DataProvider.MySqlPomelo, DataProvider.MongoDb],
                x=>x==TestDataFixture.Provider) &&
            (property == "DateOfBirth" || property == "Contract.Details.Duration"),
            "Some databases have multiple-digit default precision for seconds after string " +
            "conversion, meaning all entries will end in 00.");
        
        Assert.SkipWhen(
            Array.Exists(
                [DataProvider.SqlServer, DataProvider.MySqlPomelo], x=>x==TestDataFixture.Provider) &&
                property == "Salary",
            "Some databases have multiple-digit default precision for digits after the decimal " +
            "after string conversion, meaning all entries will end in 0.");
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.MongoDb && property == "DateOfConception",
            "Limited support for comparison on string conversions of DateTimeOffset in MongoDB");
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.MongoDb && property == "FavouriteLetter",
            "MongoDB stores char as number by default with MongoDB.EntityFrameworkCore");
        
        Assert.True(query.Count() == expectedCount);
        Assert.True(query.First().Id == expectedFirst);
    }
    
    [Theory]
    [InlineData("Id", "1", 12, 1)]
    [InlineData("Name", "lee", 1, 24)]
    [InlineData("FavouriteLetter", "e", 2, 9)]
    [InlineData("IntelligenceLevel", "4", 3, 5)]
    [InlineData("Age", "2", 10, 1)]
    [InlineData("PowerLevel", "4", 11, 1)]
    [InlineData("Stamina", "3", 1, 23)]
    [InlineData("ExperiencePoints", "12", 7, 8)]
    [InlineData("MagicPower", "123", 21, 3)]
    [InlineData("Mana", "456", 12, 16)]
    [InlineData("Strength", ".8", 3, 13, "0.00")]
    [InlineData("Agility", ".7", 2, 16, "0.00")]
    [InlineData("Salary", "3", 4, 3)]
    [InlineData("HobbitAncestry", "e", 30, 1)]
    [InlineData("Taste", "mami", 3, 1)]
    [InlineData("DateOfBirth", ":2", 5, 4, "s")]
    [InlineData("DateOfConception", "59-", 2, 19, "yyyy-MM-ddTHH:mm:sszzz")]
    [InlineData("Contract.SigningDate", "04", 1, 21, "yyyy MM dd")]
    [InlineData("Contract.Details.SigningTime", "01", 1, 21, "HH:mm:ss")]
    [InlineData("Contract.Details.Duration", "36", 1, 20, "g")]
    public void ApplyQuery_ContainsOperator_ShouldReturnEntries(
        string property,
        string value,
        int expectedCount = 1,
        int expectedFirst = 22,
        string? exactParseFormat = null,
        string? overrideCulture = null)
    {
        // Arrange
        
        QueryRequest request = new()
        {
            Filters =
            [
                new()
                {
                    Property = property,
                    Value = value,
                    Operator = FilterOperator.Contains,
                    ExactParseFormat = exactParseFormat,
                    OverrideCulture = overrideCulture
                }
            ]
        };
        
        // Act

        #region MongoDbSpecific
        
        // MongoDb branch is necessary here, since the provider currently does not support
        // evaluation of nested nullable properties
        
        if(TestDataFixture.Provider == DataProvider.MongoDb && property.StartsWith("Contract."))
        {
            int evaluationCount = 0;
            switch(property)
            {
                case "Contract.Details.Duration":
                    request.Filters.First().Property = "Duration";
                    evaluationCount = fixture.ContractDetails.ApplyQuery(request).Count();
                    break;
                case "Contract.Details.SigningTime":
                    Assert.SkipWhen(true, "MongoDb stores TimeOnly as integer, string comparisons will not work");
                    return;
                case "Contract.SigningDate":
                    request.Filters.First().Property = "SigningDate";
                    evaluationCount = fixture.Contracts.ApplyQuery(request).Count();
                    break;
            }
            
            //Assert
            
            Assert.True(evaluationCount == expectedCount);
            
            return;
        }

        #endregion
        
        IQueryable<Goblin> query = fixture.SampleData.ApplyQuery(request);
        
        // Assert
        
        Assert.SkipWhen(
            Array.Exists(
                [DataProvider.PostgreSql, DataProvider.MySqlPomelo, DataProvider.MySql],
                x=>x == TestDataFixture.Provider) &&
            Array.Exists(
                ["Contract.Details.SigningTime", "Contract.SigningDate"],
                x=>x == property),
            "The DateOnly and TimeOnly string comparison is not supported with this provider.");
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.Sqlite && property == "Mana",
            "This operator is not supported for type 'ulong' in Sqlite");
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.MongoDb && property == "DateOfConception",
            "Limited support for comparison on string conversions of DateTimeOffset in MongoDB");
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.MongoDb && property == "FavouriteLetter",
            "MongoDB stores char as number by default with MongoDB.EntityFrameworkCore");
        
        Assert.True(query.Count() == expectedCount);
        Assert.True(query.First().Id == expectedFirst);
    }
    
    [Theory]
    [InlineData("Id", "1", 18)]
    [InlineData("Name", "lee", 29)]
    [InlineData("FavouriteLetter", "e", 28)]
    [InlineData("IntelligenceLevel", "4", 27)]
    [InlineData("Age", "2", 20)]
    [InlineData("PowerLevel", "4", 19)]
    [InlineData("Stamina", "3", 29)]
    [InlineData("ExperiencePoints", "12", 23)]
    [InlineData("MagicPower", "123", 9)]
    [InlineData("Mana", "456", 18)]
    [InlineData("Strength", ".8", 27, "0.00")]
    [InlineData("Agility", ".7", 28, "0.00")]
    [InlineData("Salary", "3", 26)]
    [InlineData("HobbitAncestry", "e", 0)]
    [InlineData("Taste", "mami", 27)]
    [InlineData("DateOfBirth", ":2", 25, "s")]
    [InlineData("DateOfConception", "59-", 28, "yyyy-MM-ddTHH:mm:sszzz")]
    [InlineData("Contract.SigningDate", "04", 29, "yyyy MM dd", null, 2)]
    [InlineData("Contract.Details.SigningTime", "01", 29, "HH:mm:ss")]
    [InlineData("Contract.Details.Duration", "36", 29, "g", null, 2)]
    public void ApplyQuery_DoesNotContainOperator_ShouldReturnEntries(
        string property,
        string value,
        int expectedCount = 1,
        string? exactParseFormat = null,
        string? overrideCulture = null,
        int? mongoSpecificExpectedCount = null)
    {
        // Arrange
        
        QueryRequest request = new()
        {
            Filters =
            [
                new()
                {
                    Property = property,
                    Value = value,
                    Operator = FilterOperator.DoesNotContain,
                    ExactParseFormat = exactParseFormat,
                    OverrideCulture = overrideCulture
                }
            ]
        };
        
        // Act

        #region MongoDbSpecific
        
        // MongoDb branch is necessary here, since the provider currently does not support
        // evaluation of nested nullable properties
        
        if(TestDataFixture.Provider == DataProvider.MongoDb && property.StartsWith("Contract."))
        {
            int evaluationCount = 0;
            switch(property)
            {
                case "Contract.Details.Duration":
                    request.Filters.First().Property = "Duration";
                    evaluationCount = fixture.ContractDetails.ApplyQuery(request).Count();
                    break;
                case "Contract.Details.SigningTime":
                    Assert.SkipWhen(true, "MongoDb stores TimeOnly as integer, string comparisons will not work");
                    return;
                case "Contract.SigningDate":
                    request.Filters.First().Property = "SigningDate";
                    evaluationCount = fixture.Contracts.ApplyQuery(request).Count();
                    break;
            }
            
            //Assert
            
            Assert.True(evaluationCount == mongoSpecificExpectedCount);
            
            return;
        }

        #endregion
        
        IQueryable<Goblin> query = fixture.SampleData.ApplyQuery(request);
        
        // Assert
        
        Assert.SkipWhen(
            Array.Exists(
                [DataProvider.PostgreSql, DataProvider.MySqlPomelo, DataProvider.MySql],
                x=>x == TestDataFixture.Provider) &&
            Array.Exists(
                ["Contract.Details.SigningTime", "Contract.SigningDate"],
                x=>x == property),
            "The DateOnly and TimeOnly string comparison is not supported with this provider.");
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.Sqlite && property == "Mana",
            "This operator is not supported for type 'ulong' in Sqlite");
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.MongoDb && property == "DateOfConception",
            "Limited support for comparison on string conversions of DateTimeOffset in MongoDB");
        
        Assert.SkipWhen(
            TestDataFixture.Provider==DataProvider.MongoDb && property == "FavouriteLetter",
            "MongoDB stores char as number by default with MongoDB.EntityFrameworkCore");
        
        Assert.True(query.Count() == expectedCount);
    }
    
    [Fact]
    public void ApplyQuery_IsEmpty_ShouldReturnEntries()
    {
        // Arrange
        
        QueryRequest request = new()
        {
            Filters =
            [
                new()
                {
                    Property = "Name",
                    Operator = FilterOperator.IsEmpty,
                }
            ]
        };
        
        // Act
        
        IQueryable<Goblin> query = fixture.SampleData.ApplyQuery(request);
        
        // Assert
        
        Assert.True(query.Count() == 1);
    }
    
    [Fact]
    public void ApplyQuery_IsNotEmpty_ShouldReturnEntries()
    {
        // Arrange
        
        QueryRequest request = new()
        {
            Filters =
            [
                new()
                {
                    Property = "Name",
                    Operator = FilterOperator.IsNotEmpty,
                }
            ]
        };
        
        // Act
        
        IQueryable<Goblin> query = fixture.SampleData.ApplyQuery(request);
        
        // Assert
        
        Assert.True(query.Count() == 29);
    }
    
    [Theory]
    [InlineData("Name", "[Ee]", 9, 4)]
    [InlineData("Agility",  @"7\.", 6, 3, null, "ja-JP")]
    [InlineData("DateOfBirth", "[:]", 30, 1, "s")]
    public void ApplyQuery_Regex_ShouldReturnEntries(
        string property,
        string value,
        int expectedCount = 1,
        int expectedFirst = 22,
        string? exactParseFormat = null,
        string? overrideCulture = null)
    {
        // Arrange
        
        QueryRequest request = new()
        {
            Filters =
            [
                new()
                {
                    Property = property,
                    Value = value,
                    Operator = FilterOperator.Regex,
                    ExactParseFormat = exactParseFormat,
                    OverrideCulture = overrideCulture
                }
            ]
        };
        
        // Act
        
        IQueryable<Goblin> query = fixture.SampleData.ApplyQuery(request);
        
        // Assert
        
        Assert.SkipWhen(
            Array.Exists([DataProvider.SqlServer, DataProvider.MySql], x=>x==TestDataFixture.Provider),
            "Regex is not supported with this provider.");
        
        Assert.True(query.Count() == expectedCount);
        Assert.True(query.First().Id == expectedFirst);
    }
    
    [Fact]
    public void ApplyQuery_NullOperators_ShouldReturnEntries() 
    {
        // Arrange
        
        QueryRequest equalsRequest = new()
        {
            Filters =
            [
                new()
                {
                    Property = "Contract",
                    Operator = FilterOperator.Equals
                }
            ]
        };
        
        QueryRequest doesNotEqualRequest = new()
        {
            Filters =
            [
                new()
                {
                    Property = "Contract.Details.Duration",
                    Operator = FilterOperator.DoesNotEqual
                }
            ]
        };
        
        // Act
        
        IQueryable<Goblin> equalsQuery = fixture.SampleData.ApplyQuery(equalsRequest);
        IQueryable<Goblin> doesNotEqualQuery = fixture.SampleData.ApplyQuery(doesNotEqualRequest);
        
        // Assert
        
        Assert.SkipWhen(
            TestDataFixture.Provider == DataProvider.MongoDb,
            "MongoDB.EntityFrameworkCore does not support null evaluations.");
        
        Assert.True(equalsQuery.Count() == 27);
        Assert.True(doesNotEqualQuery.Count() == 3);
    }
    
    [Theory]
    [InlineData(FilterOperator.LessThanOrEqual, 1)]
    [InlineData(FilterOperator.GreaterThan, 0)]
    [InlineData(FilterOperator.Equals, 29, null)]
    public void ApplyQuery_NestedNullableProperty_ShouldReturnEntry(
        FilterOperator filterOperator,
        int entriesCount,
        string? value = "10") 
    {
        // Arrange
        
        QueryRequest request = new()
        {
            Filters =
            [
                new()
                {
                    Property = "Contract.Details.DaysOfEffect",
                    Value = value,
                    Operator = filterOperator,
                }
            ]
        };
        
        // Act
        
        IQueryable<Goblin> query = fixture.SampleData.ApplyQuery(request);
        
        // Assert
        
        Assert.SkipWhen(
            TestDataFixture.Provider == DataProvider.MongoDb,
            "MongoDB.EntityFrameworkCore does not support null evaluations.");
        
        Assert.True(query.Count() == entriesCount);
    }
}