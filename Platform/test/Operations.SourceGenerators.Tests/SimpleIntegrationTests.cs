using Dapper;
using Operations.Extensions.Dapper;

namespace Operations.SourceGenerators.Tests;

// Test types that will have source generation applied
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

public class SimpleIntegrationTests
{
    [Fact]
    public void SimpleRecord_GeneratesCorrectly()
    {
        var command = new SimpleTestCommand("Test", 42);
        
        // Should implement interface
        IDbParamsProvider provider = command;
        provider.ShouldNotBeNull();
        
        // Should have ToDbParams method
        var parameters = command.ToDbParams();
        parameters.ShouldNotBeNull();
        parameters.ShouldBeOfType<DynamicParameters>();
        
        // Should contain expected parameters
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
        
        // Should implement interface
        IDbParamsProvider provider = command;
        provider.ShouldNotBeNull();
        
        // Should have ToDbParams method
        var parameters = command.ToDbParams();
        parameters.ShouldNotBeNull();
        
        // Should contain snake_case parameters
        parameters.ParameterNames.ShouldContain("first_name");
        parameters.ParameterNames.ShouldContain("last_name");
        parameters.ParameterNames.ShouldContain("created_at");
    }
    
    [Fact]
    public void NestedCommand_GeneratesCorrectly()
    {
        var command = new NestedContainer.NestedCommand(Guid.NewGuid(), "test@example.com");
        
        // Should implement interface
        IDbParamsProvider provider = command;
        provider.ShouldNotBeNull();
        
        // Should have ToDbParams method
        var parameters = command.ToDbParams();
        parameters.ShouldNotBeNull();
        
        // Should contain expected parameters
        parameters.ParameterNames.ShouldContain("id");
        parameters.ParameterNames.ShouldContain("email");
    }
}