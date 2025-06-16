# Operations.Extensions.SourceGenerators

This project contains source generators for the Operations platform, providing compile-time code generation for common patterns.

## DbCommand Source Generator

The `DbCommandSourceGenerator` automatically generates code for types annotated with the `[Operations.Extensions.Dapper.DbCommandAttribute]`.

### Features

-   **`ToDbParams()` Method Generation**: Automatically generates a `ToDbParams()` method, making the annotated type implement the `Operations.Extensions.Dapper.IDbParamsProvider` interface. This method converts the command's properties into an object suitable for Dapper's parameter input.
-   **Static Handler Method Generation**: If the `Sp` (stored procedure name) or `Sql` (raw SQL query) property is provided in the `DbCommandAttribute`, a static `HandleAsync` method is generated. This method facilitates executing the database command.
    -   The signature is typically: `public static async Task<TResult> HandleAsync(YourCommandType command, DbDataSource datasource, System.Threading.CancellationToken cancellationToken = default)`
-   **Parameter Case Conversion**: Supports `ParamsCase` in `DbCommandAttribute` (e.g., `DbParamsCase.SnakeCase`) to automatically convert property names to snake_case for database parameter names within the `ToDbParams()` method. This is overridden by `[Column("custom_name")]` attribute on a property.
-   **`NonQuery` Flag Support**: The `NonQuery` flag in `DbCommandAttribute` influences the Dapper execution method used in the generated handler (e.g., `ExecuteAsync` for `true` vs. `ExecuteScalarAsync` or query methods for `false` when `TResult` is `int`).
-   **Keyed `DbDataSource` Resolution**: The `dataSource` parameter in `DbCommandAttribute` allows specifying a key for `DbDataSource` resolution. If a key is provided (e.g., `dataSource: "UserManagementWriter"`), the generated `HandleAsync` method will use the `[FromKeyedServices]` attribute to inject the keyed `DbDataSource`. If `dataSource` is not set, a default (non-keyed) `DbDataSource` parameter is used.

### Usage

#### Attribute Application

Apply the `DbCommandAttribute` to a `partial` record or class that represents your database command.

```csharp
using Operations.Extensions.Dapper;
using Operations.Extensions.Messaging; // Required for ICommand<T>

// Example 1: Command using the default DbDataSource
[DbCommand(sp: "public.create_user_default_ds", nonQuery: true)]
public partial record CreateUserDefaultDsCommand(Guid UserId, string FirstName, string LastName) : ICommand<int>;

// Example 2: Command using a specific, keyed DbDataSource
[DbCommand(sp: "public.create_user_keyed_ds", nonQuery: true, dataSource: "UserManagementWriter")]
public partial record CreateUserKeyedDsCommand(Guid UserId, string FirstName, string LastName) : ICommand<int>;

// Example 3: Command for a query expecting a result, using snake_case for parameters
[DbCommand(sql: "SELECT * FROM products WHERE product_id = @product_id;", paramsCase: DbParamsCase.SnakeCase)]
public partial record GetProductByIdQuery(int ProductId) : ICommand<Product>;
```

#### Attribute Properties

-   `Sp` (string?): The name of the stored procedure to execute. Mutually exclusive with `Sql`.
-   `Sql` (string?): The raw SQL query text to execute. Mutually exclusive with `Sp`.
-   `ParamsCase` (DbParamsCase): Specifies how property names are converted to parameter names in `ToDbParams()`. Default is `DbParamsCase.Unset` (inherits from global configuration). `DbParamsCase.SnakeCase` converts to snake_case, `DbParamsCase.None` uses property names as-is.
    -   **Global Configuration for Default Parameter Case**: When `ParamsCase` is set to `DbParamsCase.Unset` (or not specified), the actual behavior can be controlled globally via an MSBuild property in your consuming project's `.csproj` file:
        ```xml
        <PropertyGroup>
          <DbCommandDefaultParamCase>SnakeCase</DbCommandDefaultParamCase> <!-- Or None -->
        </PropertyGroup>
        ```
    -   **Interaction**:
        -   If `DbCommandDefaultParamCase` is `SnakeCase` and `ParamsCase` on the attribute is `Unset`, snake_case conversion **will be applied**.
        -   If `DbCommandDefaultParamCase` is `None` (or not set, as the generator's internal default is `None`) and `ParamsCase` is `Unset`, snake_case conversion **will not be applied** (property names used as-is).
        -   If `ParamsCase` is explicitly set to `DbParamsCase.SnakeCase` on the attribute, it **always applies** snake_case conversion, overriding the MSBuild property for that specific command.
        -   If the MSBuild property is not set by the user, the source generator defaults to `None` (no snake_case for `DbParamsCase.Unset`).
-   `nonQuery` (bool): Indicates if the command is non-query (e.g., an INSERT/UPDATE returning rows affected, typically for `ICommand<int>`). Default is `false`. If `true` for an `ICommand<int>`, `ExecuteAsync` is used. If `false` for `ICommand<int>`, `ExecuteScalarAsync<int>` is used. For other `ICommand<TResult>`, query methods are used.
-   `dataSource` (string?): An optional key for resolving a specific `DbDataSource` instance via keyed dependency injection. If provided, the generated `HandleAsync` method will use the `[FromKeyedServices]` attribute to inject the keyed `DbDataSource` directly as a parameter.

#### Generated Handler Invocation (Conceptual)

The generated static `HandleAsync` method is typically invoked by a mediating handler, command dispatcher, or DI-aware infrastructure.

```csharp
// This is conceptual; actual invocation depends on your application structure.
// The handler is static: YourCommandTypeHandler.HandleAsync(...)

// Example for CreateUserDefaultDsCommand (assuming it's ICommand<int>):
var command = new CreateUserDefaultDsCommand(Guid.NewGuid(), "Test", "User");
DbDataSource dataSource = /* resolved from DI */;
int affectedRows = await CreateUserDefaultDsCommandHandler.HandleAsync(command, dataSource);

// Example for CreateUserKeyedDsCommand (assuming it's ICommand<int>):
var keyedCommand = new CreateUserKeyedDsCommand(Guid.NewGuid(), "Test", "UserTwo");
// Note: The keyed DbDataSource is injected via [FromKeyedServices] attribute
int affectedRows = await CreateUserKeyedDsCommandHandler.HandleAsync(keyedCommand, keyedDataSource);
```
The `DbDataSource` parameter in the generated `HandleAsync` method:
- If `DbCommandAttribute.dataSource` is set (e.g., `"MyKey"`), the parameter will be decorated with `[FromKeyedServices("MyKey")]`.
- Otherwise, it will be a standard `DbDataSource` parameter for default resolution.

#### Dependency Injection for `DbDataSource`

Register your `DbDataSource` instances (default and/or keyed) with your `IServiceCollection`.

```csharp
// In your Program.cs or DI setup module:
using System.Data.Common;
using Npgsql; // Or your specific ADO.NET provider (e.g., Microsoft.Data.SqlClient)

var builder = WebApplication.CreateBuilder(args); // or Host.CreateApplicationBuilder(args);

// Default DbDataSource
builder.Services.AddSingleton<DbDataSource>(
    provider => new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("DefaultConnection")!).Build()
);

// Keyed DbDataSource for "UserManagementWriter"
builder.Services.AddKeyedSingleton<DbDataSource>("UserManagementWriter",
    (provider, key) => new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("UserDbConnection")!).Build()
);

// Keyed DbDataSource for "ReportingReader"
builder.Services.AddKeyedSingleton<DbDataSource>("ReportingReader",
    (provider, key) => new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("ReportingDbConnection")!).Build()
);
```

### Generated Code Example

#### `ToDbParams()` Method

For a command like:
`[DbCommand] public partial record MySimpleCommand(string ParamOne, int ParamTwo);`

The generator creates an implementation of `IDbParamsProvider`:
```csharp
// File: YourApp_Commands_MySimpleCommand.DbExt.g.cs
// <auto-generated/>
#nullable enable

namespace YourApp.Commands; // Assuming MySimpleCommand is in this namespace

partial record MySimpleCommand : global::Operations.Extensions.Dapper.IDbParamsProvider
{
    public global::System.Object ToDbParams()
    {
        return this;
    }
}
```

For a command with custom parameter names:
`[DbCommand(paramsCase: DbParamsCase.SnakeCase)] public partial record MySnakeCaseCommand(string FirstName, int UserId);`

The generator creates:
```csharp
// <auto-generated/>
#nullable enable

namespace YourApp.Commands;

partial record MySnakeCaseCommand : global::Operations.Extensions.Dapper.IDbParamsProvider
{
    public global::System.Object ToDbParams()
    {
        var p = new
        {
            first_name = this.FirstName,
            user_id = this.UserId
        };
        return p;
    }
}
```

#### `HandleAsync` Method (Default `DbDataSource`)

Generated for: `[DbCommand(sp: "create_user_default", nonQuery: true)] public partial record CreateUserDefaultCommand(string Name) : ICommand<int>;`

```csharp
// File: YourApp_Commands_CreateUserDefaultCommand.g.cs
// <auto-generated/>
#nullable enable

using Dapper;

namespace YourApp.Commands;

public static class CreateUserDefaultCommandHandler
{
    public static async global::System.Threading.Tasks.Task<global::System.Int32> HandleAsync(global::YourApp.Commands.CreateUserDefaultCommand command, global::System.Data.Common.DbDataSource datasource, global::System.Threading.CancellationToken cancellationToken = default)
    {
        await using var connection = await datasource.OpenConnectionAsync(cancellationToken);
        var dbParams = command.ToDbParams();
        return await global::Dapper.SqlMapper.ExecuteAsync(connection, new global::Dapper.CommandDefinition("create_user_default", dbParams, commandType: System.Data.CommandType.StoredProcedure, cancellationToken: cancellationToken));
    }
}
```

#### `HandleAsync` Method (Keyed `DbDataSource`)

Generated for: `[DbCommand(sp: "update_user_special", nonQuery: true, dataSource: "SpecialKey")] public partial record UpdateUserSpecialCommand(int UserId) : ICommand<int>;`

```csharp
// File: YourApp_Commands_UpdateUserSpecialCommand.g.cs
// <auto-generated/>
#nullable enable

using Dapper;

namespace YourApp.Commands;

public static class UpdateUserSpecialCommandHandler
{
    public static async global::System.Threading.Tasks.Task<global::System.Int32> HandleAsync(global::YourApp.Commands.UpdateUserSpecialCommand command, [global::Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute("SpecialKey")] global::System.Data.Common.DbDataSource datasource, global::System.Threading.CancellationToken cancellationToken = default)
    {
        await using var connection = await datasource.OpenConnectionAsync(cancellationToken);
        var dbParams = command.ToDbParams();
        return await global::Dapper.SqlMapper.ExecuteAsync(connection, new global::Dapper.CommandDefinition("update_user_special", dbParams, commandType: System.Data.CommandType.StoredProcedure, cancellationToken: cancellationToken));
    }
}
```

### Supported Types for `[DbCommand]` Attribute

-   **Records**: `public partial record MyCommand(...);`
-   **Classes**: `public partial class MyCommand { ... }`
    *(Structs are not typically used for commands that would involve database operations and DI, but if partial, the `ToDbParams` part could technically generate).*

### Requirements

-   Types annotated with `[DbCommand]` must be marked as `partial`.
-   If the type is nested, its containing class(es) must also be `partial`.
-   For handler generation (`sp` or `sql` specified), the command type should typically implement `Operations.Extensions.Messaging.ICommand<TResult>` for the generator to determine the return type `TResult`. If not, diagnostics may be issued, or default behavior (e.g., `Task<int>`) might apply.
-   Ensure necessary ADO.NET provider packages (e.g., `Npgsql`) and `Dapper` are referenced in the project using the generated code.
-   For keyed service injection (`dataSource` parameter used), ensure `Microsoft.Extensions.DependencyInjection.Abstractions` (or a package that includes it, like the main `Microsoft.Extensions.DependencyInjection`) is referenced.

### Snake Case Conversion Rules for `ToDbParams()`

(When `ParamsCase = DbParamsCase.SnakeCase`)
-   `FirstName` → `first_name`
-   `XMLHttpRequest` → `x_m_l_http_request`
-   `ID` → `i_d` (or `id` depending on exact behavior, typically preserves sequential capitals if followed by lowercase)
-   `UserID` → `user_i_d`

### Development

The source generator is built with:
-   **Target Framework**: `netstandard2.0` (required for source generators by Roslyn)
-   **Language Version**: C# 10 or higher recommended for generator development.
-   **Dependencies**: `Microsoft.CodeAnalysis.CSharp`, `Microsoft.CodeAnalysis.Analyzers`.

### Testing

Run tests for the source generator project (if available):
```bash
# Example: dotnet test path/to/Operations.Extensions.SourceGenerators.Tests/
```

### Troubleshooting

#### Source Generator Not Running or Not Updating
1.  Ensure the project containing the generator is correctly referenced by the consuming project with `OutputItemType="Analyzer"` and `ReferenceOutputAssembly="false"` in the `.csproj` file.
2.  Clean and rebuild the consuming project. Sometimes, restarting the IDE (Visual Studio, Rider) is necessary.
3.  Verify that target types are marked `partial`.
4.  Ensure the `[DbCommand(...)]` attribute is correctly applied and its namespace (`Operations.Extensions.Dapper`) is imported or fully qualified.

#### Compilation Errors in Generated Code
-   Make sure `Operations.Extensions.Dapper` (for `IDbParamsProvider`, `DbCommandAttribute`) and `Dapper` are referenced in the consuming project.
-   If using `dataSource` for keyed injection, ensure `Microsoft.Extensions.DependencyInjection.Abstractions` is referenced for keyed service resolution methods.
-   Check that the return type `TResult` for your command (if it implements `ICommand<TResult>`) is compatible with Dapper's expected return types (e.g., `int` for `ExecuteAsync`, your POCO for queries).

#### Generated Files Location

Generated files can be inspected to understand what the generator is producing:
-   Typically found in the consuming project's `obj/Debug/{TFM}/generated/Operations.Extensions.SourceGenerators/Operations.Extensions.SourceGenerators.DbCommand.DbCommandSourceGenerator/` directory (path may vary slightly).
-   To have these files written to disk for easier inspection, add `<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>` to a `<PropertyGroup>` in your consuming project's `.csproj` file.
-   Generated file naming patterns:
    -   For `ToDbParams()`: `{SafeFileName}.DbExt.g.cs` where `SafeFileName` is `{Namespace}_{TypeName}` (underscores replace dots in namespace).
    -   For `HandleAsync()`: `{SafeFileName}.g.cs` where the handler is generated as a static class

### Diagnostics and Analyzers

The source generator includes built-in analyzers that provide compile-time diagnostics:

#### Error Diagnostics
- **DB_COMMAND_GEN002**: Command missing ICommand<TResult> interface
    - Thrown when a class has `[DbCommand(sp: "...", ...)]` or `[DbCommand(sql: "...", ...)]` but doesn't implement `ICommand<TResult>`
    - Handler generation requires a result type to determine the appropriate Dapper method
- **DB_COMMAND_GEN003**: Both Sp and Sql specified
    - Thrown when both `Sp` and `Sql` properties are provided in the `DbCommand` attribute
    - These properties are mutually exclusive

#### Warning Diagnostics
- **DB_COMMAND_GEN001**: NonQuery attribute used with non-integral generic result
    - Thrown when `NonQuery = true` is used with a command that doesn't implement `ICommand<int>`
    - NonQuery commands should typically return an integer representing affected rows

### Architecture Notes

The source generator needs to be in a separate project because:
1.  **Analyzer Requirements**: Source generators must be referenced as analyzers.
2.  **Target Framework**: Must target `netstandard2.0` for compiler compatibility.
3.  **Isolation**: Prevents circular dependencies.
4.  **Distribution**: Can be packaged independently as a NuGet package.

This design allows the source generator to be reused, packaged, and developed independently.
