# Platform operations service

The Platform Operations Service provides enterprise infrastructure for building .NET microservices with Domain-Driven Design patterns, CQRS, event sourcing, and observability. You get production-ready services with minimal configuration through standardized extensions and source generators.

## Concept

The Platform Operations Service abstracts common microservice infrastructure into reusable components. Instead of configuring logging, health checks, messaging, and observability for each service, you add two extension methods and get everything working correctly.

The platform follows these principles:

-   **Convention over configuration** - Sensible defaults for all infrastructure
-   **Zero-allocation performance** - Source generators eliminate runtime overhead
-   **Type safety** - Compile-time validation for database operations and messaging
-   **Observability first** - Built-in tracing, metrics, and structured logging

## Example

Here's a complete microservice setup:

[!code-csharp[](~/samples/basic-service/Program.cs?highlight=4-5,9-11)]

This example configures:

-   Structured logging with Serilog
-   OpenTelemetry tracing and metrics
-   Wolverine messaging with PostgreSQL transport
-   Health checks for dependencies
-   OpenAPI documentation
-   gRPC services with HTTP/2

> [!TIP]
> Run `dotnet new webapi` and replace `Program.cs` with the example above to get started quickly.

## Service targets

The platform targets these service types:

| Service type            | Extensions                                         | Purpose                                |
| ----------------------- | -------------------------------------------------- | -------------------------------------- |
| **API services**        | `AddServiceDefaults()` + `AddApiServiceDefaults()` | REST APIs with OpenAPI, gRPC endpoints |
| **Background services** | `AddServiceDefaults()`                             | Message handlers, scheduled jobs       |
| **Worker services**     | `AddServiceDefaults()`                             | Long-running background processing     |

### API services

API services handle HTTP requests and expose REST or gRPC endpoints:

[!code-csharp[](~/samples/api-service/Program.cs)]

### Background services

Background services process messages and handle scheduled work:

[!code-csharp[](~/samples/background-service/Program.cs)]

## Core components

The platform consists of these packages:

### Operations.ServiceDefaults

Provides core infrastructure for all service types:

-   **Logging** - Serilog with structured logging and correlation IDs
-   **OpenTelemetry** - Automatic instrumentation for HTTP, database, and messaging
-   **Health checks** - Kubernetes-ready liveness and readiness endpoints
-   **Messaging** - Wolverine integration with PostgreSQL transport
-   **Configuration** - Environment-based settings with validation

[!code-csharp[](~/samples/service-defaults/ServiceConfiguration.cs)]

### Operations.ServiceDefaults.Api

Adds API-specific features on top of core infrastructure:

-   **Controllers** - MVC with model validation and error handling
-   **OpenAPI** - Swagger documentation with examples and schemas
-   **gRPC** - Protocol buffer services with reflection
-   **Authentication** - JWT token validation and authorization
-   **CORS** - Cross-origin request handling

[!code-csharp[](~/samples/api-defaults/ApiConfiguration.cs)]

### Operations.Extensions.SourceGenerators

Generates high-performance database code at compile time:

-   **Query mapping** - Zero-allocation result mapping from SQL queries
-   **Parameter binding** - Type-safe parameter generation for stored procedures
-   **Command builders** - Compile-time SQL command construction

[!code-csharp[](~/samples/source-generators/DatabaseQuery.cs)]

## Request flow

Here's how requests flow through a Platform service:

1. **HTTP request** arrives at the API service
2. **Middleware pipeline** handles authentication, logging, and error handling
3. **Controller action** validates input and calls a command or query
4. **Wolverine mediator** routes the command to the appropriate handler
5. **Handler** processes business logic and interacts with the database
6. **Source-generated code** executes SQL with zero allocations
7. **OpenTelemetry** traces the entire request across all components
8. **Response** returns to the client with proper status codes and content

> [!NOTE]
> All components automatically participate in distributed tracing, so you can follow requests across multiple services.

## Advanced customization

### Custom health checks

Add service-specific health checks:

[!code-csharp[](~/samples/customization/CustomHealthChecks.cs)]

### Message handling

Configure custom message handlers with Wolverine:

[!code-csharp[](~/samples/customization/MessageHandlers.cs)]

### Database integration

Use source generators for custom database operations:

[!code-csharp[](~/samples/customization/DatabaseOperations.cs)]

> [!WARNING]
> Source generators require partial classes and specific naming conventions. See [Source Generators Overview](content/source-generators/overview.md) for details.

## Performance considerations

The platform optimizes for high-throughput scenarios:

-   **Zero-allocation database operations** through source generators
-   **Connection pooling** with automatic health monitoring
-   **Batched telemetry** to reduce observability overhead
-   **Efficient serialization** with System.Text.Json and Protocol Buffers

Typical performance characteristics:

-   Health check responses: < 1ms
-   Database query overhead: ~5% vs. raw ADO.NET
-   Message processing: 10,000+ messages/second per instance

## Security best practices

The platform includes security defaults:

-   **HTTPS enforcement** in production environments
-   **JWT token validation** with configurable issuers
-   **Request rate limiting** to prevent abuse
-   **Sensitive data filtering** in logs and traces

> [!WARNING]
> Always configure authentication and authorization for production deployments. The platform provides infrastructure but doesn't enforce security policies.

## Additional resources

-   [Architecture overview](content/architecture/overview.md) - Core design principles and patterns
- Explore [API development](content/api/service-defaults.md) - Building REST and gRPC services
-   [Messaging patterns](content/messaging/overview.md) - Event-driven architecture with Wolverine
-   [Observability setup](content/opentelemetry/overview.md) - Monitoring and tracing configuration
-   [Source generator reference](content/source-generators/overview.md) - High-performance database operations
-   [Sample applications](https://github.com/operations-platform/samples) - Complete example services
