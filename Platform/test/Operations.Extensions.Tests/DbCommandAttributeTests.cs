// Copyright (c) ABCDEG. All rights reserved.

namespace Operations.Extensions.Tests;

public class DbCommandAttributeTests
{
    [Theory]
    [InlineData(null, null, DbParamsCase.Unset, false, null)]
    [InlineData("test_sp", null, DbParamsCase.None, true, "TestDb")]
    [InlineData(null, "SELECT * FROM users", DbParamsCase.SnakeCase, false, "ReadOnlyDb")]
    [InlineData("proc_name", "query_text", DbParamsCase.Unset, true, null)]
    public void DbCommandAttribute_WithAllParameterCombinations_ShouldSetPropertiesCorrectly(
        string? sp, string? sql, DbParamsCase paramsCase, bool nonQuery, string? dataSource)
    {
        // Act
        var attribute = new DbCommandAttribute(sp, sql, paramsCase, nonQuery, dataSource);

        // Assert
        attribute.Sp.ShouldBe(sp);
        attribute.Sql.ShouldBe(sql);
        attribute.ParamsCase.ShouldBe(paramsCase);
        attribute.NonQuery.ShouldBe(nonQuery);
        attribute.DataSource.ShouldBe(dataSource);
    }

    [Fact]
    public void DbCommandAttribute_WithSpOnly_ShouldHaveCorrectProperties()
    {
        // Act
        var attribute = new DbCommandAttribute(sp: "create_user");

        // Assert
        attribute.Sp.ShouldBe("create_user");
        attribute.Sql.ShouldBeNull();
        attribute.ParamsCase.ShouldBe(DbParamsCase.Unset);
        attribute.NonQuery.ShouldBe(false);
        attribute.DataSource.ShouldBeNull();
    }

    [Fact]
    public void DbCommandAttribute_WithSqlOnly_ShouldHaveCorrectProperties()
    {
        // Act
        var attribute = new DbCommandAttribute(sql: "SELECT * FROM users WHERE active = true");

        // Assert
        attribute.Sp.ShouldBeNull();
        attribute.Sql.ShouldBe("SELECT * FROM users WHERE active = true");
        attribute.ParamsCase.ShouldBe(DbParamsCase.Unset);
        attribute.NonQuery.ShouldBe(false);
        attribute.DataSource.ShouldBeNull();
    }
}
