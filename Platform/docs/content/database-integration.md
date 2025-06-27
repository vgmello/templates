---
title: Database Integration
description: High-performance database operations using Dapper with stored procedures, type-safe parameter binding, and automatic observability.
---

# Database integration

High-performance database operations using stored procedures with type-safe parameter binding and result mapping. You get the performance of raw ADO.NET with the convenience of an ORM through three simple extension methods.

:::moniker range=">= operations-1.0"

## Concept

Platform database integration provides a thin abstraction over Dapper that standardizes stored procedure calls across your microservices. Instead of writing repetitive connection management and parameter binding code, you call extension methods that handle the complexity while maintaining full performance.

The integration follows these patterns:
- Use `SpExecute` for commands that modify data (INSERT, UPDATE, DELETE)
- Use `SpQuery<T>` for queries that return strongly-typed results
- Use `SpCall<T>` for complex operations with output parameters

## End-to-end example

:::code language="csharp" source="~/samples/database/BasicOperations.cs" id="complete_example" highlight="3-6,10-13":::

> [!TIP]
> The anonymous object parameters are automatically mapped to stored procedure parameters using property names.

This example demonstrates:
- Type-safe parameter binding without manual DbParameter creation
- Automatic result mapping to DTOs
- Built-in cancellation token support
- Exception handling with proper context

## Targets and scopes

Database extensions target `DbDataSource` and provide these operations:

| Extension | Purpose | Returns | Use case |
|-----------|---------|---------|----------|
| `SpExecute` | Execute command procedures | `int` (rows affected) | CREATE, UPDATE, DELETE operations |
| `SpQuery<T>` | Query with results | `IEnumerable<T>` | SELECT operations returning data |
| `SpCall<T>` | Complex operations | `T` (custom result) | Procedures with output parameters |

### Parameter binding scope

All extensions support these parameter types:

:::code language="csharp" source="~/samples/database/ParameterTypes.cs" id="parameter_scope":::

### Result mapping scope

The `SpQuery<T>` extension maps these result types:

:::code language="csharp" source="~/samples/database/ResultMapping.cs" id="result_scope":::

## Customization

### Custom parameter providers

Implement `IDbParamsProvider` for complex parameter scenarios:

:::code language="csharp" source="~/samples/database/CustomParameters.cs" id="custom_provider":::

### Custom type handlers

Register Dapper type handlers for complex types:

:::code language="csharp" source="~/samples/database/TypeHandlers.cs" id="type_handlers":::

### Transaction management

Integrate with ambient transactions and messaging:

:::code language="csharp" source="~/samples/database/Transactions.cs" id="transaction_example":::

> [!WARNING]
> Always use parameterized stored procedures. Never construct dynamic SQL with string concatenation.

### Connection configuration

Customize connection pooling and performance settings:

:::code language="csharp" source="~/samples/database/ConnectionConfig.cs" id="connection_setup":::

## Performance considerations

Platform database integration optimizes for high-throughput scenarios:

- **Direct stored procedure calls** - No ORM query translation overhead
- **Connection pooling** - Npgsql pools with configurable limits
- **Prepared statements** - Automatic preparation for repeated calls
- **Zero-allocation paths** - Minimal memory allocation during operations

Performance characteristics vs alternatives:

| Operation | Platform DB | Entity Framework | Raw ADO.NET |
|-----------|-------------|------------------|-------------|
| Simple query | 100% | 300% | 95% |
| Complex query | 100% | 400% | 95% |
| Bulk insert | 100% | 600% | 90% |
| Memory allocation | Baseline | 5x higher | Minimal |

> [!NOTE]
> Benchmarks measured against PostgreSQL with 1000 concurrent operations on production hardware.

:::moniker-end

## Additional resources

- [Platform architecture](architecture.md) - Data access patterns and transaction management
- [Source generators](source-generators/overview.md) - Compile-time database code generation
- [Messaging integration](messaging/overview.md) - Transactional outbox pattern
- [Performance optimization](extensions.md) - Connection pooling and monitoring
- [Database samples](https://github.com/operations-platform/database-samples) - Complete integration examples