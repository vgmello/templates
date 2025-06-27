# Source generators

Platform source generators create high-performance database operation code at compile time. Instead of using reflection to map object properties to database parameters at runtime, the generators produce optimized code that eliminates allocations and provides compile-time validation.

## Getting started with DbCommand

The primary source generator works with the `[DbCommand]` attribute to create database command handlers. Start by marking your command class:

```csharp
[DbCommand("cashier_create")]
public partial class CreateCashierCommand : ICommand
{
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string[] Currencies { get; init; }
    public required Guid CreatedBy { get; init; }
}
```

The `partial` keyword is essential - it allows the generator to add implementation code to your class.

## Generated code overview

For the `CreateCashierCommand` above, the generator produces several pieces:

### Parameter provider implementation

```csharp
partial class CreateCashierCommand : IDbParamsProvider
{
    public void AddParameters(DynamicParameters parameters)
    {
        parameters.Add("name", Name);
        parameters.Add("email", Email);
        parameters.Add("currencies", Currencies);
        parameters.Add("created_by", CreatedBy);
    }
}
```

### Command handler interface

```csharp
public interface ICreateCashierCommandHandler
{
    Task<Result> ExecuteAsync(CreateCashierCommand command, CancellationToken cancellationToken = default);
}
```

### Service registration extensions

```csharp
public static class CreateCashierCommandExtensions
{
    public static IServiceCollection AddCreateCashierCommandHandler(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        return services.Add(new ServiceDescriptor(
            typeof(ICreateCashierCommandHandler), 
            typeof(CreateCashierCommandHandler), 
            lifetime));
    }
}
```

This generated code integrates seamlessly with dependency injection and provides zero-allocation parameter binding.

## Customizing parameter mapping

By default, property names are converted to snake_case for database parameters. Use the `[Column]` attribute to override this:

```csharp
[DbCommand("user_update")]
public partial class UpdateUserCommand : ICommand
{
    [Column("user_id")]
    public Guid Id { get; init; }
    
    [Column("full_name")]
    public string Name { get; init; } = string.Empty;
    
    // This becomes "email" automatically
    public string Email { get; init; } = string.Empty;
}
```

## Handling complex types

The generator supports various parameter types including arrays, nullable types, and JSON serialization:

```csharp
[DbCommand("invoice_create")]
public partial class CreateInvoiceCommand : ICommand
{
    public Guid CashierId { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }  // Nullable reference type
    public string[] Tags { get; init; } = [];  // Array parameter
    public DateTimeOffset? DueDate { get; init; }  // Nullable value type
    
    [Column("metadata")]
    [JsonSerialized]
    public InvoiceMetadata Metadata { get; init; } = new();  // JSON serialization
}
```

The `[JsonSerialized]` attribute tells the generator to serialize complex objects as JSON parameters.

## Generator configuration

Control the generator's behavior through MSBuild properties in your project file:

```xml
<PropertyGroup>
  <DbCommandGenerateHandlers>true</DbCommandGenerateHandlers>
  <DbCommandGenerateParams>true</DbCommandGenerateParams>
  <DbCommandDefaultSchema>billing</DbCommandDefaultSchema>
</PropertyGroup>
```

Available options:
- `DbCommandGenerateHandlers` - Generate handler interfaces and implementations
- `DbCommandGenerateParams` - Generate parameter provider implementations  
- `DbCommandDefaultSchema` - Default database schema for procedures
- `DbCommandNullableContext` - Enable nullable reference type annotations

## Integration with database operations

The generated parameter providers work directly with Platform database extensions:

```csharp
public class CashierService
{
    private readonly DbDataSource _dataSource;
    
    public async Task<Guid> CreateCashierAsync(CreateCashierCommand command)
    {
        // The command implements IDbParamsProvider automatically
        return await _dataSource.SpExecute("cashier_create", command);
    }
}
```

No manual parameter binding required - the generated code handles everything.

## Diagnostic information

The generator provides helpful diagnostics during compilation:

- **Info**: Reports discovered command classes and generated files
- **Warning**: Missing partial keyword, unsupported parameter types
- **Error**: Invalid attribute usage, conflicting configurations

Enable detailed diagnostics in your project:

```xml
<PropertyGroup>
  <DbCommandVerboseLogging>true</DbCommandVerboseLogging>
</PropertyGroup>
```

## Performance benefits

Source generation provides significant performance improvements over reflection-based approaches:

### Zero allocations

Generated parameter binding eliminates boxing and reflection overhead:

```csharp
// Generated code (zero allocations)
parameters.Add("amount", command.Amount);  // Direct value assignment

// Reflection-based approach (multiple allocations)
var property = typeof(Command).GetProperty("Amount");
var value = property.GetValue(command);
parameters.Add("amount", value);  // Boxing occurs here
```

### Compile-time validation

Parameter names and types are validated at build time:

```csharp
[DbCommand("nonexistent_procedure")]  // Build error if procedure doesn't exist
public partial class InvalidCommand : ICommand
{
    public UnsupportedType Value { get; init; }  // Build warning for unsupported type
}
```

### Optimal IL generation

The generated code produces minimal, efficient bytecode that's identical to hand-written parameter binding.

## Advanced scenarios

### Custom parameter providers

For complex scenarios, implement additional logic in your command class:

```csharp
[DbCommand("complex_operation")]
public partial class ComplexCommand : ICommand
{
    public string BasicParameter { get; init; } = string.Empty;
    
    // Generated AddParameters method is partial, so you can extend it
    partial void AddParametersCore(DynamicParameters parameters)
    {
        // Add computed or transformed parameters
        parameters.Add("computed_value", BasicParameter.ToUpperInvariant());
        parameters.Add("current_timestamp", DateTimeOffset.UtcNow);
    }
}
```

### Conditional compilation

Use preprocessor directives to generate different code for different environments:

```csharp
[DbCommand(
#if DEBUG
    "debug_procedure"
#else
    "production_procedure"
#endif
)]
public partial class EnvironmentSpecificCommand : ICommand
{
    public string Data { get; init; } = string.Empty;
}
```

## Troubleshooting

### Common issues

**Missing partial keyword**: Ensure your command class is declared as `partial`:
```csharp
// Wrong
[DbCommand("procedure")]
public class MyCommand : ICommand { }

// Correct  
[DbCommand("procedure")]
public partial class MyCommand : ICommand { }
```

**Parameter name conflicts**: Use `[Column]` to resolve naming conflicts:
```csharp
[DbCommand("user_create")]
public partial class CreateUserCommand : ICommand
{
    [Column("user_name")]  // Avoids conflict with reserved word
    public string Name { get; init; } = string.Empty;
}
```

**Unsupported types**: The generator supports most common types, but complex objects need explicit handling:
```csharp
// Unsupported
public Dictionary<string, object> Data { get; init; }

// Supported with JSON serialization
[JsonSerialized]
public MyDataObject Data { get; init; }
```

### Build output

Check the build output for generator messages:

```
DbCommand generator: Processing 5 command classes
DbCommand generator: Generated ICreateCashierCommandHandler
DbCommand generator: Generated CreateCashierCommand.g.cs
```

## See also

- [Database integration](database-integration.md)
- [Platform architecture](architecture.md)
- [.NET Source Generators documentation](https://docs.microsoft.com/dotnet/csharp/roslyn-sdk/source-generators-overview)