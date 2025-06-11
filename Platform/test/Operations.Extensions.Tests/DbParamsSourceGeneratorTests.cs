using Dapper;
using Operations.Extensions.Dapper;

namespace Operations.Extensions.Tests;

[DbParams]
public partial record SimpleTestCommand(string Name, int Value);

[DbParams]
public partial class TestClassCommand
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public partial class NestedContainer
{
    [DbParams]
    public partial record NestedCommand(Guid Id, string Email);
}

public class DbParamsSourceGeneratorTests
{
    [Fact]
    public void SimpleRecord_GeneratesCorrectly()
    {
        var command = new SimpleTestCommand("Test", 42);

        IDbParamsProvider provider = command;
        provider.ShouldNotBeNull();

        var parameters = command.ToDbParams();
        parameters.ShouldNotBeNull();
        parameters.ShouldBeOfType<DynamicParameters>();

        parameters.ParameterNames.ShouldContain("name");
        parameters.ParameterNames.ShouldContain("value");
    }

    [Fact]
    public void ClassWithProperties_GeneratesCorrectly()
    {
        var command = new TestClassCommand
        {
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.UtcNow
        };

        IDbParamsProvider provider = command;
        provider.ShouldNotBeNull();

        var parameters = command.ToDbParams();
        parameters.ShouldNotBeNull();

        parameters.ParameterNames.ShouldContain("first_name");
        parameters.ParameterNames.ShouldContain("last_name");
        parameters.ParameterNames.ShouldContain("created_at");
    }

    [Fact]
    public void NestedCommand_GeneratesCorrectly()
    {
        var command = new NestedContainer.NestedCommand(Guid.NewGuid(), "test@example.com");

        IDbParamsProvider provider = command;
        provider.ShouldNotBeNull();

        var parameters = command.ToDbParams();
        parameters.ShouldNotBeNull();

        parameters.ParameterNames.ShouldContain("id");
        parameters.ParameterNames.ShouldContain("email");
    }
}
