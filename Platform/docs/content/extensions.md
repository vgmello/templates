# Platform Extensions

## Operations.Extensions

Core utilities and extension methods for the platform.

### Result Pattern

The Result pattern provides a robust way to handle operations that may fail:

```csharp
public static Result<TValue> Success<TValue>(TValue value);
public static Result<TValue> Failure<TValue>(string error);
```

### Messaging Extensions

Extensions for message bus configuration and setup:

```csharp
public static IServiceCollection AddMessageBus(this IServiceCollection services);
```

### Dapper Extensions

Database access utilities and extensions:

```csharp
public static class DbDataSourceExtensions
{
    // Extension methods for database operations
}
```

## Operations.Extensions.Abstractions

Common interfaces and abstractions used across services.

### CQRS Interfaces

- `ICommand`: Marker interface for commands
- `IQuery<TResult>`: Interface for queries with return types

### Database Abstractions

- `IDbParamsProvider`: Interface for database parameter providers
- `ColumnAttribute`: Attribute for database column mapping
- `DbCommandAttribute`: Attribute for database command metadata

### String Extensions

Utility methods for string manipulation:

```csharp
public static class StringExtensions
{
    public static string ToCamelCase(this string input);
    public static string ToPascalCase(this string input);
    // Additional string utilities
}
```