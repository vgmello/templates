# Source Generators

The Platform Operations Service includes powerful source generators for database operations.

## DbCommand Source Generator

The `DbCommandSourceGenerator` automatically generates database command handlers and parameter providers.

### Usage

1. **Mark your command class** with the `[DbCommand]` attribute:

```csharp
[DbCommand("stored_procedure_name")]
public class CreateUserCommand : ICommand
{
    public string Name { get; set; }
    public string Email { get; set; }
}
```

2. **The generator creates**:
   - Parameter provider class
   - Command handler interface
   - Extension methods for registration

### Generated Code

For the example above, the generator produces:

```csharp
// Parameter provider
public class CreateUserCommandDbParams : IDbParamsProvider
{
    // Implementation details
}

// Handler interface
public interface ICreateUserCommandHandler
{
    Task<Result> ExecuteAsync(CreateUserCommand command, CancellationToken cancellationToken = default);
}

// Extension methods
public static class CreateUserCommandExtensions
{
    public static IServiceCollection AddCreateUserCommandHandler(this IServiceCollection services);
}
```

### Attributes

#### `[DbCommand(string procedureName)]`
Marks a class for database command generation.

#### `[Column(string name)]`
Maps a property to a specific database column name.

### Benefits

1. **Type Safety**: Compile-time validation of database parameters
2. **Performance**: Zero-allocation parameter mapping
3. **Consistency**: Standardized command patterns
4. **Maintainability**: Automatic code generation reduces boilerplate
5. **Developer Experience**: IntelliSense support for generated code

### Configuration

The source generator can be configured via MSBuild properties:

```xml
<PropertyGroup>
  <DbCommandGenerateHandlers>true</DbCommandGenerateHandlers>
  <DbCommandGenerateParams>true</DbCommandGenerateParams>
</PropertyGroup>
```