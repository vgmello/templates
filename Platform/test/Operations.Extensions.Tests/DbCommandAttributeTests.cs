// Copyright (c) ABCDEG. All rights reserved.

using Operations.Extensions.Dapper;

namespace Operations.Extensions.Tests;

public class DbCommandAttributeTests
{
    [Fact]
    public void Constructor_DefaultValues_SetsPropertiesCorrectly()
    {
        // Act
        var attribute = new DbCommandAttribute();

        // Assert
        Assert.Null(attribute.Sp);
        Assert.Null(attribute.Sql);
        Assert.Equal(DbParamsCase.Default, attribute.ParamsCase);
        Assert.False(attribute.NonQuery);
    }

    [Fact]
    public void Constructor_SpProvided_SetsSpCorrectly()
    {
        // Arrange
        var spName = "test_sp";

        // Act
        var attribute = new DbCommandAttribute(sp: spName);

        // Assert
        Assert.Equal(spName, attribute.Sp);
        Assert.Null(attribute.Sql);
        Assert.Equal(DbParamsCase.Default, attribute.ParamsCase);
        Assert.False(attribute.NonQuery);
    }

    [Fact]
    public void Constructor_SqlProvided_SetsSqlCorrectly()
    {
        // Arrange
        var sqlQuery = "SELECT * FROM Test";

        // Act
        var attribute = new DbCommandAttribute(sql: sqlQuery);

        // Assert
        Assert.Null(attribute.Sp);
        Assert.Equal(sqlQuery, attribute.Sql);
        Assert.Equal(DbParamsCase.Default, attribute.ParamsCase);
        Assert.False(attribute.NonQuery);
    }

    [Fact]
    public void Constructor_UseSnakeCaseTrue_SetsUseSnakeCaseCorrectly()
    {
        // Act
        var attribute = new DbCommandAttribute(paramsCase: DbParamsCase.SnakeCase);

        // Assert
        Assert.Equal(DbParamsCase.SnakeCase, attribute.ParamsCase);
        Assert.False(attribute.NonQuery);
    }

    [Fact]
    public void Constructor_NonQueryFalse_SetsNonQueryCorrectly() // Changed from NonQueryTrue
    {
        // Act
        var attribute = new DbCommandAttribute(nonQuery: false); // Explicitly set to false

        // Assert
        Assert.False(attribute.NonQuery);
        // Assert other defaults
        Assert.Null(attribute.Sp);
        Assert.Null(attribute.Sql);
        Assert.Equal(DbParamsCase.Default, attribute.ParamsCase);
    }

    [Fact]
    public void Constructor_AllParametersProvided_SetsAllPropertiesCorrectly() // Updated
    {
        // Arrange
        var spName = "test_sp_all";
        var useSnake = DbParamsCase.SnakeCase;
        var nonQueryVal = false; // Test explicit false for NonQuery

        // Act
        var attribute = new DbCommandAttribute(
            sp: spName,
            sql: null,
            paramsCase: useSnake,
            nonQuery: nonQueryVal);

        // Assert
        Assert.Equal(spName, attribute.Sp);
        Assert.Null(attribute.Sql);
        Assert.Equal(DbParamsCase.SnakeCase, attribute.ParamsCase);
        Assert.Equal(nonQueryVal, attribute.NonQuery);
    }

    // Removed: Constructor_AllParamsIncludingNonQuery_SetsAllPropertiesCorrectly (merged logic into updated AllParametersProvided)

    [Fact]
    public void Constructor_BothSpAndSqlProvided_ThrowsArgumentException()
    {
        // Arrange
        var spName = "test_sp";
        var sqlQuery = "SELECT * FROM Test";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new DbCommandAttribute(sp: spName, sql: sqlQuery));
        Assert.Contains("Cannot provide both 'sp' and 'sql' parameters.", exception.Message);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("some_value")]
    public void Constructor_SpOrSqlCanBeNullOrWhitespaceIfTheOtherIsNot(string? value)
    {
        // Test with default NonQuery = true
        var attr1 = new DbCommandAttribute(sp: value, sql: null);
        Assert.Equal(value, attr1.Sp);
        Assert.Null(attr1.Sql);
        Assert.True(attr1.NonQuery);

        var attr2 = new DbCommandAttribute(sp: null, sql: value);
        Assert.Null(attr2.Sp);
        Assert.Equal(value, attr2.Sql);
        Assert.True(attr2.NonQuery);

        // Test with explicit NonQuery = false
        var attr3 = new DbCommandAttribute(sp: value, sql: null, nonQuery: false);
        Assert.Equal(value, attr3.Sp);
        Assert.Null(attr3.Sql);
        Assert.False(attr3.NonQuery);
    }

    [Fact]
    public void Constructor_SpAndSqlAreNull_IsValid()
    {
        var attribute = new DbCommandAttribute(sp: null, sql: null);
        Assert.Null(attribute.Sp);
        Assert.Null(attribute.Sql);
    }

    [Fact]
    public void Constructor_SpIsEmptyAndSqlIsNull_IsValid()
    {
        var attribute = new DbCommandAttribute(sp: "", sql: null);
        Assert.Equal("", attribute.Sp);
        Assert.Null(attribute.Sql);
        Assert.True(attribute.NonQuery);
    }
}
