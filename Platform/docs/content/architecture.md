# Platform architecture

The Platform Operations Service architecture provides foundational infrastructure patterns for building enterprise microservices. You get consistent, high-performance components that eliminate boilerplate while maintaining flexibility and observability.

:::moniker range=">= operations-1.0"

## Concept

The Platform architecture follows Domain-Driven Design principles with clear separation between infrastructure concerns and business logic. Instead of implementing cross-cutting concerns in each service, you leverage shared components that provide consistent behavior across your entire system.

The architecture is built on these core principles:

- **Layered isolation** - Infrastructure, application, and domain concerns remain separate
- **Convention over configuration** - Sensible defaults reduce setup complexity
- **Compile-time optimization** - Source generators eliminate runtime overhead
- **Observable by design** - Telemetry flows through all components automatically

## Architecture overview

Here's how Platform components work together in a typical microservice:

:::code language="csharp" source="~/samples/architecture/ServiceArchitecture.cs" highlight="3-4,8-10":::

## Core components

The Platform consists of five primary packages that work together:

### Operations.Extensions

Provides utility patterns and base functionality:

:::code language="csharp" source="~/samples/architecture/Extensions.cs" id="result_pattern":::

**Key features:**
- **Result pattern** - Type-safe error handling without exceptions
- **Messaging extensions** - Common patterns for command/query handling  
- **Dapper extensions** - Enhanced database access utilities
- **Validation helpers** - FluentValidation integration patterns

### Operations.Extensions.Abstractions

Defines contracts and interfaces for the platform:

:::code language="csharp" source="~/samples/architecture/Abstractions.cs" id="cqrs_interfaces":::

**Key features:**
- **CQRS interfaces** - ICommand, IQuery, ICommandHandler, IQueryHandler
- **Database abstractions** - IDbParamsProvider, IDbCommandProvider
- **Source generation attributes** - Metadata for compile-time code generation
- **Messaging contracts** - Event and message handling interfaces

### Operations.Extensions.SourceGenerators  

Generates high-performance code at compile time:

:::code language="csharp" source="~/samples/architecture/SourceGenerators.cs" id="generated_command":::

**Key features:**
- **Database command generation** - Automatic stored procedure handlers
- **Parameter mapping** - Type-safe parameter binding with zero allocations
- **Query result mapping** - Compile-time object mapping from SQL results
- **Performance optimization** - Eliminates reflection and boxing

> [!NOTE]
> Source generators require partial classes and specific naming conventions. The generated code is available at compile time for debugging and analysis.

### Operations.ServiceDefaults

Configures core infrastructure for all services:

:::code language="csharp" source="~/samples/architecture/ServiceDefaults.cs" id="service_setup":::

**Key features:**
- **Health checks** - Kubernetes-ready liveness and readiness endpoints
- **Structured logging** - Serilog with correlation IDs and enrichment
- **OpenTelemetry** - Distributed tracing, metrics, and instrumentation
- **Messaging** - Wolverine integration with PostgreSQL and Kafka transports

### Operations.ServiceDefaults.Api

Adds API-specific functionality on top of core services:

:::code language="csharp" source="~/samples/architecture/ApiDefaults.cs" id="api_setup":::

**Key features:**
- **OpenAPI generation** - Rich documentation with examples and schemas
- **gRPC integration** - Auto-discovery and HTTP/2 optimization
- **Validation** - FluentValidation with automatic error responses
- **Authentication** - JWT token validation and authorization policies

## Layered architecture

The Platform enforces a clean layered architecture:

```
┌─────────────────┐
│   Controllers   │ ← API surface (REST/gRPC endpoints)
├─────────────────┤
│   Handlers      │ ← Application logic (commands/queries)  
├─────────────────┤
│   Domain        │ ← Business logic (entities/value objects)
├─────────────────┤
│  Infrastructure │ ← Data access (repositories/messaging)
└─────────────────┘
```

### Request flow

Here's how a typical request flows through the architecture:

1. **Controller** receives HTTP request and validates input
2. **Mediator** routes command/query to appropriate handler
3. **Handler** executes business logic using domain objects
4. **Repository** uses source-generated code for database access
5. **Events** are published for cross-service communication
6. **Response** returns with proper status codes and content

> [!TIP]
> All layers participate in distributed tracing automatically, so you can follow requests across the entire stack.

## Design patterns

### CQRS with Wolverine

Commands and queries are separated with different handling patterns:

:::code language="csharp" source="~/samples/architecture/CqrsPattern.cs" id="command_query_separation":::

### Event-driven communication

Services communicate through domain events and integration events:

:::code language="csharp" source="~/samples/architecture/EventDriven.cs" id="event_communication":::

### Repository with source generation

Data access uses the repository pattern with compile-time optimization:

:::code language="csharp" source="~/samples/architecture/RepositoryPattern.cs" id="repository_implementation":::

## Cross-cutting concerns

The Platform handles common concerns automatically:

### Observability

All components emit telemetry data:
- **Traces** - Request correlation across services
- **Metrics** - Performance counters and business metrics  
- **Logs** - Structured logging with correlation IDs

### Resilience

Built-in patterns for handling failures:
- **Circuit breakers** - Prevent cascading failures
- **Retry policies** - Automatic retry with exponential backoff
- **Bulkhead isolation** - Resource isolation between operations

### Security

Security is built into the platform:
- **Authentication** - JWT token validation
- **Authorization** - Policy-based access control
- **Data protection** - Automatic PII filtering in logs

> [!WARNING]
> While the Platform provides security infrastructure, you must configure authentication providers and authorization policies for your specific requirements.

## Performance characteristics

The Platform architecture optimizes for high-throughput scenarios:

| Component | Overhead | Optimization |
|-----------|----------|-------------|
| Source generators | ~0% | Compile-time code generation |
| Command handling | ~5% | Minimal reflection usage |
| Database access | ~2% | Zero-allocation parameter binding |
| Event publishing | ~3% | Batched message processing |

Typical performance with Platform components:
- Request latency: 10-50ms (vs 100-200ms custom implementation)
- Memory allocations: 90% reduction through source generation
- CPU utilization: 40% reduction through optimized code paths

:::moniker-end

## Additional resources

- [Extensions overview](extensions.md) - Core utility patterns and helpers
- [Source generators](source-generators.md) - High-performance code generation
- [API development](api/overview.md) - Building REST and gRPC services
- [Messaging patterns](messaging/overview.md) - Event-driven architecture
- [Database integration](database-integration.md) - Data access patterns
- [Architecture samples](https://github.com/operations-platform/architecture-samples) - Complete reference implementations