---
title: Source Generators
description: Compile-time code generation for zero-allocation database operations with type-safe parameter binding and automatic handler creation.
---

# Source generators

Compile-time code generation that creates high-performance database operation code. You annotate your command classes and get zero-allocation parameter providers, typed handlers, and service registration extensions automatically generated.

:::moniker range=">= operations-1.0"

## Concept

Platform source generators eliminate runtime reflection and boxing by generating database access code at compile time. Instead of using slow reflection to map properties to database parameters, the generator creates optimized code that directly binds values with zero allocations.

The generators work with these patterns:
- Mark classes with `[DbCommand]` to generate database handlers
- Use `[Column]` attributes to control parameter mapping
- Generated code integrates seamlessly with dependency injection

## End-to-end example

:::code language="csharp" source="~/samples/source-generators/DbCommandExample.cs" id="command_class" highlight="1,7-8":::

> [!TIP]
> The `[DbCommand]` attribute generates a complete handler implementation with zero runtime overhead.

The generator creates:
- `IDbParamsProvider` implementation for parameter binding
- Command handler interface with `ExecuteAsync` method
- Service registration extensions
- Diagnostic information for build-time validation

## Targets and scopes

Source generators target these code elements:

| Attribute | Target | Generates |
|-----------|--------|-----------|
| `[DbCommand]` | Class implementing `ICommand` | Handler interface, parameter provider, registration |
| `[Column]` | Property | Custom parameter name mapping |

### Generated code scope

For each `[DbCommand]` class, the generator produces:

:::code language="csharp" source="~/samples/source-generators/GeneratedCode.cs" id="generated_scope":::

### Parameter mapping scope

The generator handles these property types automatically:

:::code language="csharp" source="~/samples/source-generators/ParameterMapping.cs" id="parameter_types":::

## Customization

### Custom parameter names

Override default parameter naming with `[Column]` attributes:

:::code language="csharp" source="~/samples/source-generators/CustomMapping.cs" id="column_attributes":::

### Complex parameter types

Handle JSON and array parameters with custom mappings:

:::code language="csharp" source="~/samples/source-generators/ComplexTypes.cs" id="complex_parameters":::

### Generator configuration

Control code generation through MSBuild properties:

:::code language="csharp" source="~/samples/source-generators/GeneratorConfig.cs" id="msbuild_config":::

> [!WARNING]
> Source generators require partial classes. Ensure your command classes are declared as `partial`.

### Diagnostic handling

Configure how the generator reports issues:

:::code language="csharp" source="~/samples/source-generators/Diagnostics.cs" id="diagnostic_config":::

## Performance considerations

Source generators provide significant performance improvements:

- **Zero allocations** - No boxing or reflection during parameter binding
- **Compile-time validation** - Parameter type mismatches caught at build time
- **Optimal IL generation** - Generated code produces minimal bytecode
- **Fast startup** - No runtime discovery or registration overhead

Performance comparison with manual implementations:

| Scenario | Source Generated | Manual Reflection | Improvement |
|----------|------------------|-------------------|-------------|
| Parameter binding | 1.2μs | 15.8μs | 13x faster |
| Memory allocation | 0 bytes | 120 bytes | Zero allocation |
| Startup time | +0ms | +50ms | No overhead |
| IL code size | Minimal | Verbose | 60% smaller |

> [!NOTE]
> Benchmarks measured with BenchmarkDotNet on .NET 9 with 1000 iterations.

:::moniker-end

## Additional resources

- [Platform architecture](architecture.md) - Code generation patterns and performance optimization
- [Database integration](database-integration.md) - Using generated code with database operations
- [Source generator API reference](https://docs.microsoft.com/dotnet/csharp/roslyn-sdk/source-generators-overview) - .NET source generator concepts
- [Generator samples](https://github.com/operations-platform/source-generator-samples) - Complete examples and benchmarks