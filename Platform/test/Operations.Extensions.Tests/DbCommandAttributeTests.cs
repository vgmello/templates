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
    }

    [Fact]
    public void Constructor_UseSnakeCaseTrue_SetsUseSnakeCaseCorrectly()
    {
        // Act
        var attribute = new DbCommandAttribute(useSnakeCase: true);

        // Assert
        Assert.True(attribute.UseSnakeCase);
    }

    [Fact]
    public void Constructor_ReturnsAffectedRecordsFalse_SetsReturnsAffectedRecordsCorrectly()
    {
        // Act
        var attribute = new DbCommandAttribute(returnsAffectedRecords: false);

        // Assert
        Assert.False(attribute.ReturnsAffectedRecords);
    }

    [Fact]
    public void Constructor_AllParametersProvided_SetsAllPropertiesCorrectly()
    {
        // Arrange
        var spName = "test_sp_all";
        var useSnake = true;
        var returnsAffected = false;

        // Act
        // Note: Sql is omitted to ensure sp is chosen and not to trigger the mutual exclusivity error.
        var attribute = new DbCommandAttribute(sp: spName, sql: null, useSnakeCase: useSnake, returnsAffectedRecords: returnsAffected);

        // Assert
        Assert.Equal(spName, attribute.Sp);
        Assert.Null(attribute.Sql);
        Assert.Equal(useSnake, attribute.UseSnakeCase);
        Assert.Equal(returnsAffected, attribute.ReturnsAffectedRecords);
    }

    [Fact]
    public void Constructor_BothSpAndSqlProvided_ThrowsArgumentException()
    {
        // Arrange
        var spName = "test_sp";
        var sqlQuery = "SELECT * FROM Test";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new DbCommandAttribute(sp: spName, sql: sqlQuery));
        // Check if the message contains the relevant part, rather than exact match for flexibility.
        Assert.Contains("Cannot provide both 'sp' and 'sql' parameters.", exception.Message);
    }

    [Theory]
    [InlineData(" ")] // Whitespace SP/SQL
    [InlineData("some_value")] // Valid SP/SQL
    // Null case for sp/sql is covered by default constructor or when one is set and other is explicitly null.
    public void Constructor_SpOrSqlCanBeNullOrWhitespaceIfTheOtherIsNot(string? value)
    {
        // Case 1: sp is value, sql is null (explicitly)
        var attr1 = new DbCommandAttribute(sp: value, sql: null);
        Assert.Equal(value, attr1.Sp);
        Assert.Null(attr1.Sql);

        // Case 2: sql is value, sp is null (explicitly)
        var attr2 = new DbCommandAttribute(sp: null, sql: value);
        Assert.Null(attr2.Sp);
        Assert.Equal(value, attr2.Sql);
    }

    [Fact]
    public void Constructor_SpAndSqlAreNull_IsValid()
    {
        // This case is for when [DbCommand] is used only to configure ToDbParams behavior (e.g. UseSnakeCase)
        // Act
        var attribute = new DbCommandAttribute(sp: null, sql: null, useSnakeCase: true);

        // Assert
        Assert.Null(attribute.Sp);
        Assert.Null(attribute.Sql);
        Assert.True(attribute.UseSnakeCase); // Ensure other params are still processed
    }

    [Fact]
    public void Constructor_SpIsEmptyAndSqlIsNull_IsValid()
    {
        // Act
        var attribute = new DbCommandAttribute(sp: "", sql: null, useSnakeCase: true);

        // Assert
        Assert.Equal("", attribute.Sp); // Empty is a valid value (though generator might ignore it for handler)
        Assert.Null(attribute.Sql);
        Assert.True(attribute.UseSnakeCase);
    }
}
