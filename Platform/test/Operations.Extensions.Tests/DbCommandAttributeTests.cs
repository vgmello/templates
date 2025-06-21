// Copyright (c) ABCDEG. All rights reserved.

using Operations.Extensions.Abstractions.Dapper;

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
        Assert.Equal(DbParamsCase.Unset, attribute.ParamsCase);
        Assert.False(attribute.NonQuery);
        Assert.Null(attribute.DataSource); // New assertion
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
        Assert.Equal(DbParamsCase.Unset, attribute.ParamsCase);
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
        Assert.Equal(DbParamsCase.Unset, attribute.ParamsCase);
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
        Assert.Equal(DbParamsCase.Unset, attribute.ParamsCase);
        Assert.Null(attribute.DataSource); // New assertion
    }

    [Fact]
    public void Constructor_AllParametersProvided_SetsAllPropertiesCorrectly() // Updated
    {
        // Arrange
        var spName = "test_sp_all";
        var useSnake = DbParamsCase.SnakeCase;
        var nonQueryVal = false; // Test explicit false for NonQuery
        var dataSourceVal = "MyKey";

        // Act
        var attribute = new DbCommandAttribute(
            sp: spName,
            sql: null,
            paramsCase: useSnake,
            nonQuery: nonQueryVal,
            dataSource: dataSourceVal); // New parameter

        // Assert
        Assert.Equal(spName, attribute.Sp);
        Assert.Null(attribute.Sql);
        Assert.Equal(DbParamsCase.SnakeCase, attribute.ParamsCase);
        Assert.Equal(nonQueryVal, attribute.NonQuery);
        Assert.Equal(dataSourceVal, attribute.DataSource); // New assertion
    }

    [Fact]
    public void Constructor_DataSourceProvided_SetsDataSourceCorrectly()
    {
        // Arrange
        var dataSourceVal = "TestDataSource";

        // Act
        var attribute = new DbCommandAttribute(dataSource: dataSourceVal);

        // Assert
        Assert.Equal(dataSourceVal, attribute.DataSource);
        // Assert other defaults
        Assert.Null(attribute.Sp);
        Assert.Null(attribute.Sql);
        Assert.Equal(DbParamsCase.Unset, attribute.ParamsCase);
        Assert.False(attribute.NonQuery);
    }

    [Fact]
    public void Constructor_DefaultValues_DataSourceIsNull() // Explicit test for default DataSource
    {
        // Act
        var attribute = new DbCommandAttribute();

        // Assert
        Assert.Null(attribute.DataSource);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("some_value")]
    public void Constructor_SpOrSqlCanBeNullOrWhitespaceIfTheOtherIsNot(string? value)
    {
        // Test with default NonQuery = false (as per original class design before potential prior edits)
        var attr1 = new DbCommandAttribute(sp: value, sql: null);
        Assert.Equal(value, attr1.Sp);
        Assert.Null(attr1.Sql);
        Assert.False(attr1.NonQuery); // Assuming default is false
        Assert.Null(attr1.DataSource);

        var attr2 = new DbCommandAttribute(sp: null, sql: value);
        Assert.Null(attr2.Sp);
        Assert.Equal(value, attr2.Sql);
        Assert.False(attr2.NonQuery); // Assuming default is false
        Assert.Null(attr2.DataSource);

        // Test with explicit NonQuery = true
        var attr3 = new DbCommandAttribute(sp: value, sql: null, nonQuery: true);
        Assert.Equal(value, attr3.Sp);
        Assert.Null(attr3.Sql);
        Assert.True(attr3.NonQuery); // Explicitly true
        Assert.Null(attr3.DataSource);
    }

    [Fact]
    public void Constructor_SpAndSqlAreNull_IsValid()
    {
        var attribute = new DbCommandAttribute(sp: null, sql: null);
        Assert.Null(attribute.Sp);
        Assert.Null(attribute.Sql);
        Assert.Null(attribute.DataSource);
    }

    [Fact]
    public void Constructor_SpIsEmptyAndSqlIsNull_IsValid()
    {
        var attribute = new DbCommandAttribute(sp: "", sql: null);
        Assert.Equal("", attribute.Sp);
        Assert.Null(attribute.Sql);
        Assert.False(attribute.NonQuery); // Assuming default is false
        Assert.Null(attribute.DataSource);
    }
}
