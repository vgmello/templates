---
title: Extensions
description: Essential utility libraries providing Result patterns, string extensions, and core abstractions for Platform development.
---

# Extensions

The Platform Extensions provide essential utility libraries that form the foundation for other Platform components. These include the Result pattern for error handling, performant string manipulation utilities, and core abstractions for messaging and database access.

## Quick start

Use Platform extensions to write more expressive and type-safe code:

[!code-csharp[](~/samples/extensions/ResultPattern.cs#ResultUsage)]

The extensions provide:

-   **Result pattern** for error handling without exceptions
-   **String utilities** for efficient case conversion
-   **Core abstractions** for messaging and database operations
-   **Performance-optimized** implementations
-   **Nullable reference** type support

## Result pattern

### Type-safe error handling

Handle success and failure scenarios explicitly:

[!code-csharp[](~/samples/extensions/ResultPattern.cs#ResultUsage)]

Result benefits:

-   **Explicit error handling** without exceptions
-   **Type safety** for success and failure cases
-   **Composable operations** with functional patterns
-   **Performance benefits** by avoiding exception overhead

### Validation integration

Seamlessly integrate with FluentValidation:

[!code-csharp[](~/samples/extensions/ResultPattern.cs#ValidationIntegration)]

Validation features:

-   **Automatic conversion** from ValidationResult
-   **Multiple validation** errors support
-   **Structured error** information
-   **Consistent error** handling patterns

### Pattern matching

Use pattern matching for clean error handling:

[!code-csharp[](~/samples/extensions/ResultPattern.cs#PatternMatching)]

Pattern matching provides:

-   **Readable code** for handling different outcomes
-   **Exhaustive checking** at compile time
-   **Type-safe access** to success values
-   **Structured error** processing

## String extensions

### Case conversion utilities

Efficient string case conversion for database and API conventions:

[!code-csharp[](~/samples/extensions/StringExtensions.cs#CaseConversion)]

Case conversion features:

-   **snake_case** conversion for database columns
-   **kebab-case** conversion for URLs and APIs
-   **Performance-optimized** with stack allocation
-   **Unicode-aware** character handling
-   **Memory-efficient** implementation

### Advanced transformation

Handle complex case conversion scenarios:

[!code-csharp[](~/samples/extensions/AdvancedTransformation.cs)]

Advanced features:

-   **Acronym handling** (e.g., "XMLParser" → "xml_parser")
-   **Number sequences** (e.g., "Version2API" → "version2_api")
-   **Custom separators** for different conventions
-   **Edge case handling** for empty and null strings

### Performance characteristics

High-performance implementation details:

[!code-csharp[](~/samples/extensions/PerformanceCharacteristics.cs)]

Performance benefits:

-   **Stack allocation** for small strings
-   **Single-pass** conversion algorithm
-   **Minimal allocations** for memory efficiency
-   **O(n) complexity** for optimal scaling

## Core abstractions

### Messaging interfaces

Define command and query contracts:

[!code-csharp[](~/samples/extensions/MessagingInterfaces.cs)]

Messaging abstractions:

-   **ICommand<T>** for write operations
-   **IQuery<T>** for read operations
-   **Generic return types** for type safety
-   **Handler discovery** support

### Database abstractions

Core interfaces for database operations:

[!code-csharp[](~/samples/extensions/DatabaseAbstractions.cs)]

Database abstractions:

-   **IDbParamsProvider** for parameter mapping
-   **Attribute-based** configuration
-   **Source generator** integration
-   **Type-safe** parameter binding

### Column mapping

Control database column name mapping:

[!code-csharp[](~/samples/extensions/ColumnMapping.cs)]

Column mapping features:

-   **Custom column names** with attributes
-   **Case conversion** override capability
-   **Flexible naming** strategies
-   **Metadata-driven** configuration

## Integration patterns

### Service registration

Register extension utilities with dependency injection:

[!code-csharp[](~/samples/extensions/ServiceRegistration.cs)]

### Configuration binding

Bind extension settings from configuration:

[!code-csharp[](~/samples/extensions/ConfigurationBinding.cs)]

### Middleware integration

Use extensions within middleware pipelines:

[!code-csharp[](~/samples/extensions/MiddlewareIntegration.cs)]

## Performance optimization

### Memory management

Efficient memory usage patterns:

[!code-csharp[](~/samples/extensions/MemoryManagement.cs)]

Memory optimizations:

-   **Span<T> usage** for zero-allocation operations
-   **Object pooling** for frequently used objects
-   **Stack allocation** for small buffers
-   **GC pressure** reduction techniques

### Benchmarking results

Performance comparison with alternatives:

[!code-csharp[](~/samples/extensions/BenchmarkingResults.cs)]

Benchmark highlights:

-   **3x faster** than Regex-based conversion
-   **50% fewer** allocations than alternatives
-   **Zero allocations** for strings under 128 characters
-   **Linear scaling** with input size

## Testing utilities

### Unit testing extensions

Test utilities for extension methods:

[!code-csharp[](~/samples/extensions/UnitTestingExtensions.cs)]

### Property-based testing

Validate extension behavior with property tests:

[!code-csharp[](~/samples/extensions/PropertyBasedTesting.cs)]

### Performance testing

Verify performance characteristics:

[!code-csharp[](~/samples/extensions/PerformanceTesting.cs)]

## Common scenarios

### API response handling

Use Result pattern for API responses:

[!code-csharp[](~/samples/extensions/ApiResponseHandling.cs)]

### Database parameter mapping

Convert C# properties to database parameters:

[!code-csharp[](~/samples/extensions/DatabaseParameterMapping.cs)]

### Configuration naming

Convert configuration keys to different conventions:

[!code-csharp[](~/samples/extensions/ConfigurationNaming.cs)]

## Best practices

-   **Use Result<T>** instead of throwing exceptions for expected errors
-   **Leverage string extensions** for consistent naming conventions
-   **Implement IDbParamsProvider** for database command parameters
-   **Follow naming conventions** with case conversion utilities
-   **Test edge cases** thoroughly for string transformations
-   **Consider performance** implications of string operations
-   **Use pattern matching** for readable Result handling

## Error handling

### Result error patterns

Handle different types of errors with Result:

[!code-csharp[](~/samples/extensions/ResultErrorPatterns.cs)]

### Exception conversion

Convert exceptions to Result types:

[!code-csharp[](~/samples/extensions/ExceptionConversion.cs)]

### Validation aggregation

Combine multiple validation results:

[!code-csharp[](~/samples/extensions/ValidationAggregation.cs)]

## Migration strategies

### From exception-based code

Gradually adopt Result pattern:

[!code-csharp[](~/samples/extensions/MigrationFromExceptions.cs)]

### From string manipulation libraries

Replace existing string utilities:

[!code-csharp[](~/samples/extensions/MigrationFromStringLibraries.cs)]

## Advanced usage

### Custom Result types

Create domain-specific Result implementations:

[!code-csharp[](~/samples/extensions/CustomResultTypes.cs)]

### Extension method chaining

Chain string operations efficiently:

[!code-csharp[](~/samples/extensions/ExtensionMethodChaining.cs)]

### Generic constraints

Use generic constraints for type safety:

[!code-csharp[](~/samples/extensions/GenericConstraints.cs)]

## Next steps

-   Learn about [String Extensions](string-extensions.md) in detail
-   Explore [Result Pattern](result-pattern.md) implementation
-   Understand [Database Integration](../database-integration/overview.md) usage
-   Review [Messaging](../messaging/overview.md) abstractions

## Additional resources

-   [OneOf Library](https://github.com/mcintyre321/OneOf) - Used for Result implementation
-   [FluentValidation](https://fluentvalidation.net/) - Validation framework integration
-   [System.Memory](https://learn.microsoft.com/en-us/dotnet/api/system.memory) - High-performance memory operations
-   [String Performance](https://learn.microsoft.com/en-us/dotnet/standard/base-types/best-practices-strings) - .NET string best practices
