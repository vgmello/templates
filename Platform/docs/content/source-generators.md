---
title: Source generators
description: Compile-time code generation for high-performance database operations and improved developer experience
---

# Source generators

Platform source generators create optimized database operation code at compile time, eliminating runtime reflection overhead and providing compile-time validation. The generators produce type-safe parameter binding and command handlers that integrate seamlessly with your application's dependency injection container.

## What are source generators?

Source generators analyze your code during compilation and automatically generate additional source files. The Platform's `DbCommand` source generator specifically targets database command classes marked with the `[DbCommand]` attribute, producing:

- Parameter provider implementations (`IDbParamsProvider`)
- Static command handlers with proper `DbDataSource` injection
- Type-safe parameter binding with configurable naming conventions

This approach delivers zero-allocation parameter binding and compile-time validation of database operations.

## Basic usage

Mark your command class with the `[DbCommand]` attribute and declare it as `partial`:

:::code language="csharp" source="~/samples/billing/CreateCashierCommand.cs" id="basic_command":::

The `partial` keyword allows the generator to add implementation code to your class. The generator creates several components:

:::code language="csharp" source="~/samples/billing/CreateCashierCommand.g.cs" id="generated_implementation":::

> [!TIP]
> The generated `ToDbParams()` method converts property names to snake_case by default when `DbParamsCase.SnakeCase` is specified.

## Command handler generation

When you specify a stored procedure name, the generator creates a static handler method:

:::code language="csharp" source="~/samples/billing/CreateUserCommand.cs" id="handler_command":::

The generated handler includes proper connection management and parameter binding:

:::code language="csharp" source="~/samples/billing/CreateUserCommand.g.cs" id="generated_handler":::

## Parameter name conversion

Control how property names map to database parameters using the `ParamsCase` property:

:::code language="csharp" source="~/samples/billing/ParameterMappingExamples.cs" id="parameter_cases":::

### Global parameter case configuration

Set the default parameter case conversion for all commands in your project:

```xml
<PropertyGroup>
  <DbCommandDefaultParamCase>SnakeCase</DbCommandDefaultParamCase>
</PropertyGroup>
```

Available options:
- `SnakeCase`: Converts `FirstName` to `first_name`
- `None`: Uses property names as-is

## Custom parameter names

Override individual parameter names using the `[Column]` attribute:

:::code language="csharp" source="~/samples/billing/CustomParameterNames.cs" id="column_attributes":::

## Keyed database connections

Use different database connections for different commands by specifying a `dataSource` key:

:::code language="csharp" source="~/samples/billing/KeyedDataSource.cs" id="keyed_datasource":::

Register keyed data sources in your dependency injection container:

:::code language="csharp" source="~/samples/billing/ServiceRegistration.cs" id="keyed_services":::

The generated handler automatically uses the keyed service:

:::code language="csharp" source="~/samples/billing/KeyedDataSource.g.cs" id="keyed_handler":::

## Query vs. non-query operations

The `nonQuery` flag determines which Dapper method to use:

:::code language="csharp" source="~/samples/billing/QueryTypes.cs" id="query_types":::

- `nonQuery: true` → Uses `ExecuteAsync` (returns affected row count)
- `nonQuery: false` → Uses `ExecuteScalarAsync<T>` or query methods

## Advanced parameter handling

### Complex types with JSON serialization

Serialize complex objects as JSON parameters:

:::code language="csharp" source="~/samples/billing/JsonSerialization.cs" id="json_parameters":::

### Nullable and array parameters

The generator handles various parameter types automatically:

:::code language="csharp" source="~/samples/billing/ComplexTypes.cs" id="complex_types":::

## Configuration options

Configure the generator's behavior through MSBuild properties:

```xml
<PropertyGroup>
  <DbCommandDefaultParamCase>SnakeCase</DbCommandDefaultParamCase>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
</PropertyGroup>
```

## Generated file inspection

View generated files to understand the generator's output:

- **Location**: `obj/Debug/{TFM}/generated/Operations.Extensions.SourceGenerators/`
- **Naming**: `{Namespace}_{TypeName}.DbExt.g.cs` for parameter providers
- **Handlers**: `{Namespace}_{TypeName}Handler.g.cs` for command handlers

Enable file emission for easier debugging:

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
</PropertyGroup>
```

## Error diagnostics

The generator provides compile-time validation:

### DB_COMMAND_GEN002: Missing ICommand interface
```csharp
// Error: Handler generation requires ICommand<TResult>
[DbCommand(sp: "create_user")]
public partial class CreateUserCommand { } // Missing ICommand<TResult>

// Fixed:
[DbCommand(sp: "create_user", nonQuery: true)]
public partial class CreateUserCommand : ICommand<int> { }
```

### DB_COMMAND_GEN003: Conflicting SQL specifications
```csharp
// Error: Cannot specify both Sp and Sql
[DbCommand(sp: "create_user", sql: "INSERT INTO users...")]
public partial class CreateUserCommand : ICommand<int> { }

// Fixed: Use one or the other
[DbCommand(sp: "create_user")]
public partial class CreateUserCommand : ICommand<int> { }
```

## Performance benefits

Source generation delivers significant performance improvements:

### Zero allocations
Generated code eliminates boxing and reflection overhead:

```csharp
// Generated code (zero allocations)
var p = new { name = command.Name, email = command.Email };

// Reflection-based approach (multiple allocations)
var property = typeof(Command).GetProperty("Name");
var value = property.GetValue(command); // Boxing occurs
```

### Compile-time validation
Parameter types and names are validated during build, catching errors before runtime.

### Optimal IL generation
The generated code produces efficient bytecode equivalent to hand-written parameter binding.

## Troubleshooting

### Missing partial keyword
Ensure command classes are declared as `partial`:

```csharp
// Incorrect
[DbCommand("procedure")]
public class MyCommand : ICommand<int> { }

// Correct
[DbCommand("procedure")]
public partial class MyCommand : ICommand<int> { }
```

### Generator not running
1. Clean and rebuild the project
2. Restart your IDE if necessary
3. Verify the generator package is properly referenced
4. Check that `[DbCommand]` namespace is imported

### Missing dependencies
Ensure required packages are referenced:
- `Operations.Extensions` for `IDbParamsProvider`
- `Dapper` for database operations
- `Microsoft.Extensions.DependencyInjection.Abstractions` for keyed services

## See also

- [Database integration](database-integration.md)
- [Platform architecture](architecture.md)
- [.NET Source Generators overview](https://learn.microsoft.com/dotnet/csharp/roslyn-sdk/source-generators-overview)