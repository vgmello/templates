# Operations.SourceGenerators

This project contains source generators for the Operations platform, providing compile-time code generation for common patterns.

## DbParams Source Generator

The `DbParamsIncrementalGenerator` automatically generates `ToDbParams()` methods for types annotated with the `[DbParams]` attribute.

### Features

- **Automatic Interface Implementation**: Generated classes implement `IDbParamsProvider`
- **Snake Case Conversion**: Property names are automatically converted to snake_case for database parameters
- **Incremental Generation**: Uses the modern incremental source generator approach for better performance
- **Nested Type Support**: Works with nested classes and records

### Usage

1. **Add the attribute** to your command/query types:

```csharp
using Operations.Extensions.Dapper;

[DbParams]
public partial record CreateUserCommand(string FirstName, string LastName, string Email);
```

2. **Reference the source generator** in your project file:

```xml
<ItemGroup>
    <ProjectReference Include="path/to/Operations.SourceGenerators.csproj" 
                      OutputItemType="Analyzer" 
                      ReferenceOutputAssembly="false" />
</ItemGroup>
```

3. **Use with NpgsqlDataSourceExtensions**:

```csharp
public async Task<int> CreateUser(CreateUserCommand command, NpgsqlDataSource dataSource)
{
    // The source generator creates ToDbParams() method and IDbParamsProvider implementation
    return await dataSource.CallSp("create_user", command, cancellationToken);
}
```

### Generated Code Example

For the example above, the source generator creates:

```csharp
partial record CreateUserCommand : Operations.Extensions.Dapper.IDbParamsProvider
{
    public Dapper.DynamicParameters ToDbParams()
    {
        var p = new Dapper.DynamicParameters();
        p.Add("first_name", FirstName);
        p.Add("last_name", LastName);
        p.Add("email", Email);
        return p;
    }
}
```

### Supported Types

- **Records**: `public partial record MyCommand(...)`
- **Classes**: `public partial class MyCommand { ... }`
- **Structs**: `public partial struct MyCommand { ... }`
- **Nested Types**: Types nested within static partial classes

### Requirements

- Types must be marked as `partial`
- Containing classes (for nested types) must be `static partial`
- Properties will be mapped to snake_case parameter names

### Snake Case Conversion Rules

- `FirstName` → `first_name`
- `XMLHttpRequest` → `x_m_l_http_request`
- `ID` → `i_d`
- `UserID` → `user_i_d`

### Development

The source generator is built with:
- **Target Framework**: netstandard2.0 (required for source generators)
- **Language Version**: Latest C# features
- **Dependencies**: Microsoft.CodeAnalysis.CSharp

### Testing

Run tests with:

```bash
dotnet test Platform/test/Operations.SourceGenerators.Tests/
```

### Troubleshooting

#### Source Generator Not Running

1. Ensure the project reference has `OutputItemType="Analyzer"` and `ReferenceOutputAssembly="false"`
2. Clean and rebuild the consuming project
3. Check that types are marked as `partial`
4. Verify the `[DbParams]` attribute is applied

#### Compilation Errors

- Make sure `Operations.Extensions` is referenced for the `IDbParamsProvider` interface
- Ensure containing classes are `static partial` for nested types
- Check that the `Operations.Extensions.Dapper` namespace is imported

#### Generated Files Location

Generated files can be found in:
- `obj/Debug/net9.0/generated/Operations.SourceGenerators/...`
- Enable `<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>` in your project to see generated files
- Files are named with pattern: `{FullTypeName}.DbParams.g.cs`

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