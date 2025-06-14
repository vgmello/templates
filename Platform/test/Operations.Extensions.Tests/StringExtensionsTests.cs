// Copyright (c) ABCDEG. All rights reserved.

using Operations.Extensions.SourceGenerators.Extensions;
using Shouldly;
using Xunit;

namespace Operations.Extensions.Tests;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("FirstName", "first_name")]
    [InlineData("LastName", "last_name")]
    [InlineData("XMLHttpRequest", "x_m_l_http_request")]
    [InlineData("ID", "i_d")]
    [InlineData("UserID", "user_i_d")]
    [InlineData("HTTPSConnection", "h_t_t_p_s_connection")]
    [InlineData("APIKey", "a_p_i_key")]
    [InlineData("Name", "name")]
    [InlineData("ALLCAPS", "a_l_l_c_a_p_s")]
    [InlineData("lowercase", "lowercase")]
    [InlineData("MixedCASEString", "mixed_c_a_s_e_string")]
    [InlineData("", "")]
    [InlineData("A", "a")]
    [InlineData("AB", "a_b")]
    [InlineData("Ab", "ab")]
    [InlineData("aB", "a_b")]
    public void ToSnakeCase_ShouldConvertCorrectly(string input, string expected)
    {
        // Act
        var result = input.ToSnakeCase();

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void ToSnakeCase_WithNullInput_ShouldReturnNull()
    {
        // Act
        var result = ((string)null!).ToSnakeCase();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void ToSnakeCase_WithEmptyString_ShouldReturnEmpty()
    {
        // Act
        var result = string.Empty.ToSnakeCase();

        // Assert
        result.ShouldBe(string.Empty);
    }

    [Theory]
    [InlineData("AlreadyHas_Underscores", "already_has_underscores")]
    [InlineData("Mixed_CaseWith_Underscores", "mixed_case_with_underscores")]
    [InlineData("_StartsWithUnderscore", "_starts_with_underscore")]
    [InlineData("EndsWithUnderscore_", "ends_with_underscore_")]
    [InlineData("Multiple__Underscores", "multiple__underscores")]
    public void ToSnakeCase_WithExistingUnderscores_ShouldHandleCorrectly(string input, string expected)
    {
        // Act
        var result = input.ToSnakeCase();

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("snake_case", "snake_case")]
    [InlineData("already_in_snake_case", "already_in_snake_case")]
    [InlineData("123numbers", "123numbers")]
    [InlineData("with123Numbers", "with123_numbers")]
    [InlineData("Numbers123AtEnd", "numbers123_at_end")]
    public void ToSnakeCase_WithVariousEdgeCases_ShouldHandleCorrectly(string input, string expected)
    {
        // Act
        var result = input.ToSnakeCase();

        // Assert
        result.ShouldBe(expected);
    }
}