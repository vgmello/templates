# Operations.SourceGenerators

This project contains source generators for the Operations platform, providing compile-time code generation for common patterns.

## DbCommand Source Generator

The `DbParamsIncrementalGenerator` (class name of the generator, to be potentially renamed for clarity in the future, e.g., to `DbCommandIncrementalGenerator`) automatically generates code for types annotated with the `[Operations.Extensions.Dapper.DbCommandAttribute]`.

### Features

-   **`ToDbParams()` Method Generation**: Automatically generates a `ToDbParams()` method, making the annotated type implement the `Operations.Extensions.Dapper.IDbParamsProvider` interface. This method converts the command's properties into an object suitable for Dapper's parameter input.
-   **Static Handler Method Generation**: If the `Sp` (stored procedure name) or `Sql` (raw SQL query) property is provided in the `DbCommandAttribute`, a static `HandleAsync` method is generated. This method facilitates executing the database command.
    -   The signature is typically: `public static async Task<TResult> HandleAsync(YourCommandType command, System.Data.Common.DbDataSource dataSource, System.Threading.CancellationToken cancellationToken = default)`
-   **Parameter Case Conversion**: Supports `ParamsCase` in `DbCommandAttribute` (e.g., `DbParamsCase.SnakeCase`) to automatically convert property names to snake_case for database parameter names within the `ToDbParams()` method. This is overridden by `[Column("custom_name")]` attribute on a property.
-   **`NonQuery` Flag Support**: The `NonQuery` flag in `DbCommandAttribute` influences the Dapper execution method used in the generated handler (e.g., `ExecuteAsync` for `true` vs. `ExecuteScalarAsync` or query methods for `false` when `TResult` is `int`).
-   **Keyed `DbDataSource` Injection**: The `DataSource` property in `DbCommandAttribute` allows specifying a key for `DbDataSource` resolution. If a key is provided (e.g., `DataSource = "UserManagementWriter"`), the generated `HandleAsync` method's `DbDataSource` parameter will be annotated with `[Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute("UserManagementWriter")]`, enabling dependency injection of a specific, named data source. If `DataSource` is not set, a default (non-keyed) `DbDataSource` is expected.

### Usage

#### Attribute Application

Apply the `DbCommandAttribute` to a `partial` record or class that represents your database command.

```csharp
using Operations.Extensions.Dapper;
using System.Data.Common; // Required for DbDataSource parameter in generated HandleAsync
using Microsoft.Extensions.DependencyInjection; // Required for [FromKeyedServices]

// Example 1: Command using the default DbDataSource
[DbCommand(sp: "public.create_user_default_ds", NonQuery = true)]
public partial record CreateUserDefaultDsCommand(Guid UserId, string FirstName, string LastName);

// Example 2: Command using a specific, keyed DbDataSource
[DbCommand(sp: "public.create_user_keyed_ds", NonQuery = true, DataSource = "UserManagementWriter")]
public partial record CreateUserKeyedDsCommand(Guid UserId, string FirstName, string LastName);

// Example 3: Command for a query expecting a result, using snake_case for parameters
[DbCommand(sql: "SELECT * FROM products WHERE product_id = @product_id;", ParamsCase = DbParamsCase.SnakeCase)]
public partial record GetProductByIdQuery(int ProductId); // Assuming ICommand<Product> or similar
```

#### Attribute Properties

-   `Sp` (string?): The name of the stored procedure to execute. Mutually exclusive with `Sql`.
-   `Sql` (string?): The raw SQL query text to execute. Mutually exclusive with `Sp`.
-   `ParamsCase` (DbParamsCase): Specifies how property names are converted to parameter names in `ToDbParams()`. Default is `DbParamsCase.Default` (uses property names as-is). `DbParamsCase.SnakeCase` converts to snake_case.
    -   **Global Configuration for Default Snake Case**: When `ParamsCase` is set to `DbParamsCase.Default` (or not specified), the actual behavior can be controlled globally via an MSBuild property in your consuming project's `.csproj` file:
        ```xml
        <PropertyGroup>
          <OperationsDbCommandDefaultToSnakeCase>true</OperationsDbCommandDefaultToSnakeCase> <!-- Or false -->
        </PropertyGroup>
        ```
    -   **Interaction**:
        -   If `OperationsDbCommandDefaultToSnakeCase` is `true` and `ParamsCase` on the attribute is `Default`, snake_case conversion **will be applied**.
        -   If `OperationsDbCommandDefaultToSnakeCase` is `false` (or not set, as the generator's internal default is `false`) and `ParamsCase` is `Default`, snake_case conversion **will not be applied** (property names used as-is).
        -   If `ParamsCase` is explicitly set to `DbParamsCase.SnakeCase` on the attribute, it **always applies** snake_case conversion, overriding the MSBuild property for that specific command.
        -   If the MSBuild property is not set by the user, the source generator defaults to `false` (no snake_case for `DbParamsCase.Default`). The associated `.props` file (if the generator is packaged as a NuGet) might also provide a default value for this property.
-   `NonQuery` (bool): Indicates if the command is non-query (e.g., an INSERT/UPDATE returning rows affected, typically for `ICommand<int>`). Default is `false`. If `true` for an `ICommand<int>`, `ExecuteAsync` is used. If `false` for `ICommand<int>`, `ExecuteScalarAsync<int>` is used. For other `ICommand<TResult>`, query methods are used.
-   `DataSource` (string?): An optional key for resolving a specific `DbDataSource` instance via keyed dependency injection services. If provided, the generated `HandleAsync` method's `DbDataSource` parameter will use `[FromKeyedServices("your_key")]`.

#### Generated Handler Invocation (Conceptual)

The generated static `HandleAsync` method is typically invoked by a mediating handler, command dispatcher, or DI-aware infrastructure.

```csharp
// This is conceptual; actual invocation depends on your application structure.
// The handler is static: YourCommandTypeHandler.HandleAsync(...)

// Example for CreateUserDefaultDsCommand (assuming it's ICommand<int>):
// var command = new CreateUserDefaultDsCommand(Guid.NewGuid(), "Test", "User");
// DbDataSource defaultDataSource = /* resolve default DbDataSource from DI */;
// int affectedRows = await CreateUserDefaultDsCommandHandler.HandleAsync(command, defaultDataSource);

// Example for CreateUserKeyedDsCommand (assuming it's ICommand<int>):
// var keyedCommand = new CreateUserKeyedDsCommand(Guid.NewGuid(), "Test", "UserTwo");
// DbDataSource userManagementDataSource = /* resolve keyed "UserManagementWriter" DbDataSource from DI */;
// int affectedRows = await CreateUserKeyedDsCommandHandler.HandleAsync(keyedCommand, userManagementDataSource);
```
The `DbDataSource` parameter in the generated `HandleAsync` method is intended for dependency injection.
- If `DbCommandAttribute.DataSource` is set (e.g., `"MyKey"`), the parameter will be `[FromKeyedServices("MyKey")] DbDataSource dataSource`.
- Otherwise, it will be `DbDataSource dataSource`, expecting a default registration.

#### Dependency Injection for `DbDataSource`

Register your `DbDataSource` instances (default and/or keyed) with your `IServiceCollection`.

```csharp
// In your Program.cs or DI setup module:
// using System.Data.Common;
// using Npgsql; // Or your specific ADO.NET provider (e.g., Microsoft.Data.SqlClient)

var builder = WebApplication.CreateBuilder(args); // or Host.CreateApplicationBuilder(args);

// Default DbDataSource
builder.Services.AddSingleton<DbDataSource>(
    new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("DefaultConnection")).Build()
);

// Keyed DbDataSource for "UserManagementWriter"
builder.Services.AddKeyedSingleton<DbDataSource>("UserManagementWriter",
    (serviceProvider, key) => new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("UserDbConnection")).Build()
);

// Keyed DbDataSource for "ReportingReader"
builder.Services.AddKeyedSingleton<DbDataSource>("ReportingReader",
    (serviceProvider, key) => new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("ReportingDbConnection")).Build()
);
```

### Generated Code Example

#### `ToDbParams()` Method

For a command like:
`[DbCommand] public partial record MySimpleCommand(string ParamOne, int ParamTwo);`

The generator creates an implementation of `IDbParamsProvider`:
```csharp
// File: Namespace_MySimpleCommand.DbOps.g.cs (if nested, path reflects nesting)
// <auto-generated/>
#nullable enable

namespace YourApp.Commands; // Assuming MySimpleCommand is in this namespace

partial record MySimpleCommand : global::Operations.Extensions.Dapper.IDbParamsProvider
{
    public global::System.Object ToDbParams()
    {
        var p = new {
            ParamOne = this.ParamOne,
            ParamTwo = this.ParamTwo
        };
        return p;
    }
}
```
*Note: The actual generated object for parameters is an anonymous type. Dapper can work with these.*

#### `HandleAsync` Method (Default `DbDataSource`)

Generated for: `[DbCommand(sp: "create_user_default")] public partial record CreateUserDefaultCommand(string Name);`
(Assuming `CreateUserDefaultCommand` implements `ICommand<int>`)

```csharp
// File: YourApp.Commands.CreateUserDefaultCommand.CreateUserDefaultCommandHandler.g.cs (example path)
// <auto-generated/>
#nullable enable

using System.Threading.Tasks;
using Dapper;
using System.Data;
using System.Data.Common;
// Microsoft.Extensions.DependencyInjection is NOT included here if DataSource key is null/empty

namespace YourApp.Commands; // Assuming CreateUserDefaultCommand is in this namespace

public static class CreateUserDefaultCommandHandler
{
    public static async Task<int> HandleAsync(
        global::YourApp.Commands.CreateUserDefaultCommand command,
        global::System.Data.Common.DbDataSource dataSource, // Default DbDataSource injection
        global::System.Threading.CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var dbParams = command.ToDbParams();
        // Example Dapper call for a stored procedure expecting an int result (non-query = true)
        return await connection.ExecuteAsync(new CommandDefinition("create_user_default", dbParams, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
    }
}
```

#### `HandleAsync` Method (Keyed `DbDataSource`)

Generated for: `[DbCommand(sp: "update_user_special", DataSource = "SpecialKey")] public partial record UpdateUserSpecialCommand(int UserId);`
(Assuming `UpdateUserSpecialCommand` implements `ICommand<int>`)
```csharp
// File: YourApp.Commands.UpdateUserSpecialCommand.UpdateUserSpecialCommandHandler.g.cs (example path)
// <auto-generated/>
#nullable enable

using System.Threading.Tasks;
using Dapper;
using System.Data;
using System.Data.Common;
using Microsoft.Extensions.DependencyInjection; // Added because DataSource key is present

namespace YourApp.Commands; // Assuming UpdateUserSpecialCommand is in this namespace

public static class UpdateUserSpecialCommandHandler
{
    public static async Task<int> HandleAsync(
        global::YourApp.Commands.UpdateUserSpecialCommand command,
        [global::Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute("SpecialKey")]
        global::System.Data.Common.DbDataSource dataSource, // Keyed DbDataSource injection
        global::System.Threading.CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var dbParams = command.ToDbParams();
        // Example Dapper call for a stored procedure
        return await connection.ExecuteAsync(new CommandDefinition("update_user_special", dbParams, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
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
-   For handler generation (`Sp` or `Sql` specified), the command type should typically implement `Operations.Extensions.Messaging.ICommand<TResult>` for the generator to determine the return type `TResult`. If not, diagnostics may be issued, or default behavior (e.g., `Task<int>`) might apply.
-   Ensure necessary ADO.NET provider packages (e.g., `Npgsql`) and `Dapper` are referenced in the project using the generated code.
-   For keyed service injection (`DataSource` property used), ensure `Microsoft.Extensions.DependencyInjection.Abstractions` (or a package that includes it, like the main `Microsoft.Extensions.DependencyInjection`) is referenced.

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
-   If using `DataSource` for keyed injection, ensure a package providing `Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute` (like `Microsoft.Extensions.DependencyInjection.Abstractions`) is referenced.
-   Check that the return type `TResult` for your command (if it implements `ICommand<TResult>`) is compatible with Dapper's expected return types (e.g., `int` for `ExecuteAsync`, your POCO for queries).

#### Generated Files Location

Generated files can be inspected to understand what the generator is producing:
-   Typically found in the consuming project's `obj/Debug/{TFM}/generated/Operations.Extensions.SourceGenerators/Operations.Extensions.SourceGenerators.DbCommand.DbCommandSourceGenerator/` directory (path may vary slightly).
-   To have these files written to disk for easier inspection, add `<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>` to a `<PropertyGroup>` in your consuming project's `.csproj` file.
-   Generated file naming patterns:
    -   For `ToDbParams()`: `{Namespace}_{ContainingType}_{TypeName}.DbOps.g.cs` (underscores replace dots in namespace/containing types).
    -   For `HandleAsync()`: `{Namespace}_{ContainingType}_{TypeName}.{TypeName}Handler.g.cs`. If not nested, it's simpler, e.g., `Global_{TypeName}.{TypeName}Handler.g.cs`. (The exact pattern for handlers might vary slightly based on nesting).

### Architecture Notes

(This section can largely remain as-is if it describes general source generator project structure reasons, but ensure it doesn't contradict the specifics of `DbCommandSourceGenerator`.)

The source generator needs to be in a separate project because:
1.  **Analyzer Requirements**: Source generators must be referenced as analyzers.
2.  **Target Framework**: Must target `netstandard2.0` for compiler compatibility.
3.  **Isolation**: Prevents circular dependencies.
4.  **Distribution**: Can be packaged independently as a NuGet package.

This design allows the source generator to be reused, packaged, and developed independently.