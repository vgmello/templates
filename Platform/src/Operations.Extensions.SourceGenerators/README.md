# Operations.Extensions.SourceGenerators

This project contains source generators for the Operations platform, providing compile-time code generation for common patterns.

## DbCommand Source Generator

The `DbCommandSourceGenerator` automatically generates database interaction code for types annotated with the `[DbCommand]` attribute, including `ToDbParams()` methods and command handlers.

### Features

- **Dual Code Generation**: Generates both `ToDbParams()` methods and command handler classes
- **Database Operations**: Supports both stored procedures and SQL queries
- **Parameter Conversion**: Automatic snake_case conversion with `[Column]` attribute override support
- **Command Integration**: Works with `ICommand<TResult>` for CQRS pattern integration
- **Type Safety**: Generates strongly-typed handlers with proper return types
- **Diagnostics**: Comprehensive compile-time validation and error reporting
- **Incremental Generation**: Modern incremental source generator for optimal performance
- **Nested Type Support**: Full support for nested classes and records

### Usage

1. **Add the attribute** to your command/query types:

```csharp
using Operations.Extensions.Dapper;
using Operations.Extensions.Messaging;

// For stored procedure with integer result
[DbCommand(sp: "billing.create_user", nonQuery: true, paramsCase: DbParamsCase.SnakeCase)]
public partial record CreateUserCommand(string FirstName, string LastName, string Email) : ICommand<int>;

// For SQL query returning data
[DbCommand(sql: "SELECT * FROM users WHERE active = @active", paramsCase: DbParamsCase.SnakeCase)]
public partial record GetActiveUsersQuery(bool Active) : ICommand<IEnumerable<User>>;
```

2. **Reference the source generator** in your project file:

```xml
<ItemGroup>
    <ProjectReference Include="path/to/Operations.Extensions.SourceGenerators.csproj" 
                      OutputItemType="Analyzer" 
                      ReferenceOutputAssembly="false" />
</ItemGroup>
```

3. **Use the generated handlers**:

```csharp
public async Task<int> CreateUser(CreateUserCommand command, NpgsqlDataSource dataSource, CancellationToken cancellationToken)
{
    // The source generator creates both ToDbParams() method and handler
    return await CreateUserCommandHandler.HandleAsync(command, dataSource, cancellationToken);
}

public async Task<IEnumerable<User>> GetActiveUsers(GetActiveUsersQuery query, NpgsqlDataSource dataSource)
{
    return await GetActiveUsersQueryHandler.HandleAsync(query, dataSource);
}
```

### Generated Code Example

For the `CreateUserCommand` above, the source generator creates two files:

**1. DbOps implementation (CreateUserCommand.DbOps.g.cs)**:
```csharp
partial record CreateUserCommand : global::Operations.Extensions.Dapper.IDbParamsProvider
{
    public global::System.Object ToDbParams()
    {
        var p = new {
            first_name = this.FirstName,
            last_name = this.LastName,
            email = this.Email
        };
        return p;
    }
}
```

**2. Handler class (CreateUserCommand.CreateUserCommandHandler.g.cs)**:
```csharp
public static class CreateUserCommandHandler
{
    public static async Task<int> HandleAsync(CreateUserCommand command, global::Npgsql.NpgsqlDataSource dataSource, global::System.Threading.CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var dbParams = command.ToDbParams();
        return await connection.ExecuteAsync(new CommandDefinition("billing.create_user", dbParams, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
    }
}
```

### Supported Command Types

- **Non-Query Commands**: `ICommand<int>`, `ICommand<long>` with `nonQuery: true`
- **Scalar Queries**: `ICommand<int>`, `ICommand<string>`, etc. for single values
- **Collection Queries**: `ICommand<IEnumerable<T>>` for multiple results
- **Single Object Queries**: `ICommand<User>` for single entity results

### Supported Type Declarations

- **Records**: `public partial record MyCommand(...) : ICommand<TResult>`
- **Classes**: `public partial class MyCommand : ICommand<TResult> { ... }`
- **Structs**: `public partial struct MyCommand : ICommand<TResult> { ... }`
- **Nested Types**: Types nested within static partial classes

### Attribute Configuration

```csharp
[DbCommand(
    sp: "schema.stored_procedure_name",     // Stored procedure name
    sql: "SELECT * FROM table WHERE ...",  // Raw SQL (alternative to sp)
    paramsCase: DbParamsCase.SnakeCase,    // Parameter naming convention
    nonQuery: true                          // For INSERT/UPDATE/DELETE operations
)]
```

### Parameter Case Conversion

- **DbParamsCase.PascalCase**: Property names used as-is
- **DbParamsCase.SnakeCase**: Automatic conversion (FirstName → first_name)
- **[Column("custom_name")]**: Override individual property names

### Requirements

- Types must be marked as `partial`
- Must implement `ICommand<TResult>` for handler generation
- Containing classes (for nested types) must be `static partial`
- Either `sp` or `sql` parameter must be specified for handler generation

### Snake Case Conversion Rules

- `FirstName` → `first_name`
- `XMLHttpRequest` → `x_m_l_http_request`
- `ID` → `i_d`
- `UserID` → `user_i_d`

### Diagnostics

The source generator provides compile-time validation:

- **DBCOMMANDGEN001 (Warning)**: NonQuery used with non-integral result type
- **DBCOMMANDGEN002 (Error)**: Missing ICommand<TResult> interface when sp/sql specified

### Development

The source generator is built with:
- **Target Framework**: netstandard2.0 (required for source generators)
- **Language Version**: Latest C# features
- **Dependencies**: Microsoft.CodeAnalysis.CSharp, PolySharp

### Testing

Run tests with:

```bash
dotnet test Platform/test/Operations.Extensions.SourceGenerators.Tests/
```

### Troubleshooting

#### Source Generator Not Running

1. Ensure the project reference has `OutputItemType="Analyzer"` and `ReferenceOutputAssembly="false"`
2. Clean and rebuild the consuming project
3. Check that types are marked as `partial`
4. Verify the `[DbCommand]` attribute is applied correctly
5. Ensure `ICommand<TResult>` is implemented for handler generation

#### Compilation Errors

- Make sure `Operations.Extensions` is referenced for the `IDbParamsProvider` interface
- Ensure `Operations.Extensions.Messaging` is referenced for `ICommand<TResult>`
- Ensure containing classes are `static partial` for nested types
- Check that the `Operations.Extensions.Dapper` namespace is imported
- Verify either `sp` or `sql` parameter is provided in the attribute

#### Generated Files Location

Generated files can be found in:
- `obj/Debug/net9.0/generated/Operations.Extensions.SourceGenerators/...`
- Enable `<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>` in your project to see generated files
- Files are named with pattern: `{FullTypeName}.DbOps.g.cs` and `{FullTypeName}.{HandlerName}.g.cs`

### Architecture Notes

The source generator needs to be in a separate project because:

1. **Analyzer Requirements**: Source generators must be referenced as analyzers, not regular project references
2. **Target Framework**: Must target netstandard2.0 for compatibility with the compiler
3. **Isolation**: Prevents circular dependencies and ensures proper build ordering
4. **Distribution**: Can be packaged and distributed independently

This design allows the source generator to be:
- Reused across multiple projects in the solution
- Packaged as a NuGet package if needed
- Developed and tested independently
- Used without adding runtime dependencies to consuming projects