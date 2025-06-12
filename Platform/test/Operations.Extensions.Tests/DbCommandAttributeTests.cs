// Copyright (c) ABCDEG. All rights reserved.

using System;
using Operations.Extensions.Dapper; // Your DbCommandAttribute namespace
using Xunit;

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
        Assert.False(attribute.UseSnakeCase);
        Assert.True(attribute.ReturnsAffectedRecords); // Default for returnsAffectedRecords is true
        Assert.False(attribute.NonQuery); // Default for NonQuery is false
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
        Assert.False(attribute.UseSnakeCase);
        Assert.True(attribute.ReturnsAffectedRecords);
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
        Assert.False(attribute.UseSnakeCase);
        Assert.True(attribute.ReturnsAffectedRecords);
        Assert.False(attribute.NonQuery);
    }

    [Fact]
    public void Constructor_UseSnakeCaseTrue_SetsUseSnakeCaseCorrectly()
    {
        // Act
        var attribute = new DbCommandAttribute(useSnakeCase: true);

        // Assert
        Assert.True(attribute.UseSnakeCase);
        Assert.False(attribute.NonQuery); // Check NonQuery default
    }

    [Fact]
    public void Constructor_ReturnsAffectedRecordsFalse_SetsReturnsAffectedRecordsCorrectly()
    {
        // Act
        var attribute = new DbCommandAttribute(returnsAffectedRecords: false);

        // Assert
        Assert.False(attribute.ReturnsAffectedRecords);
        Assert.False(attribute.NonQuery); // Check NonQuery default
    }

    [Fact]
    public void Constructor_NonQueryTrue_SetsNonQueryCorrectly()
    {
        // Act
        var attribute = new DbCommandAttribute(nonQuery: true);

        // Assert
        Assert.True(attribute.NonQuery);
        // Assert other defaults
        Assert.Null(attribute.Sp);
        Assert.Null(attribute.Sql);
        Assert.False(attribute.UseSnakeCase);
        Assert.True(attribute.ReturnsAffectedRecords);
    }

    [Fact]
    public void Constructor_AllParametersProvided_SetsAllPropertiesCorrectly() // Old test, ensure NonQuery default is checked or set
    {
        // Arrange
        var spName = "test_sp_all";
        var useSnake = true;
        var returnsAffected = false;
        // bool nonQuery = false; // Assuming default if not specified for this test variant

        // Act
        var attribute = new DbCommandAttribute(sp: spName, sql: null, useSnakeCase: useSnake, returnsAffectedRecords: returnsAffected /* nonQuery default is false */);

        // Assert
        Assert.Equal(spName, attribute.Sp);
        Assert.Null(attribute.Sql);
        Assert.Equal(useSnake, attribute.UseSnakeCase);
        Assert.Equal(returnsAffected, attribute.ReturnsAffectedRecords);
        Assert.False(attribute.NonQuery); // Default NonQuery
    }

    [Fact]
    public void Constructor_AllParamsIncludingNonQuery_SetsAllPropertiesCorrectly() // New specific test for NonQuery
    {
        // Arrange
        var spName = "test_sp_all_nq";
        var useSnake = true;
        var returnsAffected = false; // Explicitly set to false
        var nonQueryVal = true;

        // Act
        var attribute = new DbCommandAttribute(
            sp: spName,
            sql: null,
            useSnakeCase: useSnake,
            returnsAffectedRecords: returnsAffected,
            nonQuery: nonQueryVal);

        // Assert
        Assert.Equal(spName, attribute.Sp);
        Assert.Null(attribute.Sql);
        Assert.Equal(useSnake, attribute.UseSnakeCase);
        Assert.Equal(returnsAffected, attribute.ReturnsAffectedRecords); // Will be stored
        Assert.Equal(nonQueryVal, attribute.NonQuery); // Will be stored
    }

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
        var attr1 = new DbCommandAttribute(sp: value, sql: null);
        Assert.Equal(value, attr1.Sp);
        Assert.Null(attr1.Sql);
        Assert.False(attr1.NonQuery);

        var attr2 = new DbCommandAttribute(sp: null, sql: value);
        Assert.Null(attr2.Sp);
        Assert.Equal(value, attr2.Sql);
        Assert.False(attr2.NonQuery);
    }

    [Fact]
    public void Constructor_SpAndSqlAreNull_IsValid()
    {
        var attribute = new DbCommandAttribute(sp: null, sql: null, useSnakeCase: true);
        Assert.Null(attribute.Sp);
        Assert.Null(attribute.Sql);
        Assert.True(attribute.UseSnakeCase);
        Assert.False(attribute.NonQuery);
    }

    [Fact]
    public void Constructor_SpIsEmptyAndSqlIsNull_IsValid()
    {
        var attribute = new DbCommandAttribute(sp: "", sql: null, useSnakeCase: true);
        Assert.Equal("", attribute.Sp);
        Assert.Null(attribute.Sql);
        Assert.True(attribute.UseSnakeCase);
        Assert.False(attribute.NonQuery);
    }
}
