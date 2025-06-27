---
title: Source Generators
description: Compile-time code generation for database operations with DbCommand source generator, reducing boilerplate and ensuring type safety.
ms.date: 01/27/2025
---

# Source Generators

The Platform provides powerful source generators that automatically generate code for common patterns, eliminating boilerplate while maintaining compile-time safety and optimal performance. The primary focus is on database operations through the DbCommand source generator.

## Quick start

Apply the `[DbCommand]` attribute to generate database access code automatically:

[!code-csharp[](~/samples/source-generators/QuickStart.cs#BasicDbCommand)]

This generates:
- **Parameter mapping** method (`ToDbParams()`)
- **Command handler** for stored procedure execution
- **Type-safe** database operations
- **Zero runtime overhead** with compile-time generation

## DbCommand source generator

### Automatic code generation
The DbCommand generator creates comprehensive database access code:

[!code-csharp[](~/samples/source-generators/DbCommandExamples.cs#StoredProcedureCommand)]

Generated code includes:
- Implementation of `IDbParamsProvider` interface
- Static `HandleAsync` method for command execution
- Proper parameter mapping with case conversion
- Connection management and error handling

### Parameter case conversion
Automatically convert C# property names to database conventions:

[!code-csharp[](~/samples/source-generators/DbCommandExamples.cs#SnakeCaseConversion)]

Case conversion features:
- **snake_case** conversion for database parameters
- **Custom column names** with `[Column]` attribute override
- **Global configuration** via MSBuild properties
- **Per-command override** with attribute parameters

### SQL query support
Generate code for both stored procedures and SQL queries:

[!code-csharp[](~/samples/source-generators/DbCommandExamples.cs#SqlQueryCommand)]

SQL support includes:
- **Raw SQL queries** with parameter substitution
- **Function calls** with automatic parameter appending
- **Query vs non-query** execution modes
- **Type-safe result** mapping

## Generated code structure

### Parameter provider implementation
Automatic implementation of database parameter interface:

[!code-csharp[](~/samples/source-generators/GeneratedCode.cs#ParameterProvider)]

### Command handler generation
Static handler methods for command execution:

[!code-csharp[](~/samples/source-generators/GeneratedCode.cs#CommandHandler)]

### Keyed data source support
Support for multiple database connections:

[!code-csharp[](~/samples/source-generators/GeneratedCode.cs#KeyedDataSource)]

## Configuration options

### Global MSBuild configuration
Configure default behavior project-wide:

```xml
<PropertyGroup>
  <DbCommandDefaultParamCase>SnakeCase</DbCommandDefaultParamCase>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
</PropertyGroup>
```

### Attribute parameters
Fine-tune generation per command:

[!code-csharp[](~/samples/source-generators/AttributeParameters.cs#AttributeOptions)]

Attribute parameters include:
- **sp**: Stored procedure name
- **sql**: Raw SQL query text
- **fn**: Function call with auto-parameters
- **paramsCase**: Parameter name conversion
- **nonQuery**: Execution mode (query vs non-query)
- **dataSource**: Named data source key

## Service registration

### Single data source
Register default database connection:

[!code-csharp[](~/samples/source-generators/ServiceRegistration.cs#SingleDataSource)]

### Multiple data sources
Support multiple databases with keyed services:

[!code-csharp[](~/samples/source-generators/ServiceRegistration.cs#MultipleDataSources)]

### Handler integration
Integrate generated handlers with dependency injection:

[!code-csharp[](~/samples/source-generators/ServiceRegistration.cs#HandlerIntegration)]

## Advanced scenarios

### Complex type mapping
Handle complex parameter scenarios:

[!code-csharp[](~/samples/source-generators/ComplexTypes.cs#ComplexMapping)]

### Bulk operations
Generate efficient bulk operation handlers:

[!code-csharp[](~/samples/source-generators/BulkOperations.cs#BulkCommands)]

### Custom result types
Support complex result type mapping:

[!code-csharp[](~/samples/source-generators/CustomResultTypes.cs#CustomResults)]

## Error handling and diagnostics

### Compile-time validation
Built-in analyzers provide immediate feedback:

[!code-csharp[](~/samples/source-generators/CompileTimeValidation.cs#DiagnosticExamples)]

Diagnostic categories:
- **DB_COMMAND_GEN001**: NonQuery with non-integral result warning
- **DB_COMMAND_GEN002**: Missing ICommand<TResult> interface error
- **DB_COMMAND_GEN003**: Conflicting Sp and Sql properties error

### Generated file inspection
View generated code for debugging:

```bash
# Enable generated file output
<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>

# Generated files location
obj/Debug/net9.0/generated/Operations.Extensions.SourceGenerators/
```

### Troubleshooting common issues
Resolve typical source generation problems:

[!code-csharp[](~/samples/source-generators/Troubleshooting.cs#CommonIssues)]

## Performance characteristics

### Compile-time optimization
Source generation provides significant benefits:

[!code-csharp[](~/samples/source-generators/PerformanceCharacteristics.cs#CompileTimeOptimization)]

Performance benefits:
- **Zero runtime overhead** from code generation
- **Optimal IL code** without reflection
- **Type safety** with compile-time validation
- **Minimal memory** allocations

### Benchmarking results
Performance comparison with alternatives:

[!code-csharp[](~/samples/source-generators/BenchmarkResults.cs#PerformanceComparison)]

## Testing generated code

### Unit testing approaches
Test generated functionality effectively:

[!code-csharp[](~/samples/source-generators/TestingGenerated.cs#UnitTesting)]

Testing strategies:
- **Integration tests** with real database
- **Mock testing** for isolation
- **Generated code** verification
- **Performance testing** of generated methods

### Test data builders
Create test data efficiently:

[!code-csharp[](~/samples/source-generators/TestDataBuilders.cs#DataBuilders)]

## Migration strategies

### From manual Dapper code
Gradually adopt source generation:

[!code-csharp[](~/samples/source-generators/Migration.cs#FromManualDapper)]

### From Entity Framework
Migrate performance-critical code:

[!code-csharp[](~/samples/source-generators/Migration.cs#FromEntityFramework)]

## Best practices

- **Use partial types** for all attributed classes and records
- **Implement ICommand<T>** for proper return type inference
- **Choose appropriate execution modes** (query vs non-query)
- **Use keyed data sources** for multi-database scenarios
- **Follow naming conventions** for consistent parameter mapping
- **Enable generated file output** during development
- **Test generated code** thoroughly
- **Monitor compilation performance** with large numbers of commands

## Advanced customization

### Custom analyzers
Extend validation with custom analyzers:

[!code-csharp[](~/samples/source-generators/CustomAnalyzers.cs#CustomValidation)]

### Generator configuration
Advanced generator configuration options:

[!code-csharp[](~/samples/source-generators/GeneratorConfiguration.cs#AdvancedConfig)]

### Plugin architecture
Extend generation capabilities:

[!code-csharp[](~/samples/source-generators/PluginArchitecture.cs#ExtensibilityPoints)]

## Next steps

- Learn about [DbCommand Generator](dbcommand-generator.md) in detail
- Explore [Database Integration](../database-integration/overview.md) patterns
- Understand [Extensions](../extensions/overview.md) for supporting utilities
- Review [Architecture](../architecture/overview.md) for design principles

## Additional resources

- [.NET Source Generators](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview)
- [Roslyn Analyzers](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview)
- [Source Generator Cookbook](https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md)
- [Source Generator Samples](https://github.com/dotnet/roslyn-sdk/tree/main/samples/CSharp/SourceGenerators)