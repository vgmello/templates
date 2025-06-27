---
title: Platform Extensions
description: Extension methods that provide production-ready infrastructure for .NET microservices with minimal configuration.
---

# Platform extensions

Extension methods that configure logging, messaging, health checks, and API capabilities for .NET microservices. You get production-ready infrastructure with a few method calls instead of hundreds of lines of boilerplate configuration.

:::moniker range=">= operations-1.0"

## Concept

Platform extensions eliminate infrastructure setup complexity by providing opinionated defaults for microservice concerns. Each extension method configures multiple related services with consistent patterns across your entire system.

The extensions follow composition patterns where you build up capabilities:
- Start with `AddServiceDefaults()` for core infrastructure
- Add `AddApiServiceDefaults()` for web APIs
- Include messaging, validation, or custom features as needed

## End-to-end example

:::code language="csharp" source="~/samples/extensions/CompleteService.cs" id="complete_setup" highlight="3-4,8-10":::

> [!TIP]
> The `AddServiceDefaults()` call configures logging, OpenTelemetry, health checks, and messaging in one line.

This example sets up:
- Structured logging with Serilog
- OpenTelemetry metrics and distributed tracing
- Health checks with Kubernetes endpoints
- Wolverine messaging with PostgreSQL transport
- OpenAPI documentation with Scalar UI
- gRPC auto-discovery

## Targets and scopes

Platform extensions target these service builder types:

| Extension | Target | Configures |
|-----------|--------|------------|
| `AddServiceDefaults()` | `WebApplicationBuilder` | Logging, telemetry, messaging, health checks |
| `AddApiServiceDefaults()` | `WebApplicationBuilder` | Controllers, OpenAPI, gRPC, validation |
| `ConfigureApiUsingDefaults()` | `WebApplication` | Middleware pipeline, endpoints |
| `MapGrpcServices()` | `WebApplication` | gRPC service auto-discovery |
| `MapDefaultHealthCheckEndpoints()` | `WebApplication` | Health check endpoints |

### Service infrastructure scope

The `AddServiceDefaults()` extension configures:

:::code language="csharp" source="~/samples/extensions/ServiceScope.cs" id="service_defaults_scope":::

### API infrastructure scope

The `AddApiServiceDefaults()` extension adds:

:::code language="csharp" source="~/samples/extensions/ApiScope.cs" id="api_defaults_scope":::

## Customization

### Custom service configuration

Override defaults through configuration or options patterns:

:::code language="csharp" source="~/samples/extensions/CustomConfiguration.cs" id="custom_config":::

### Custom middleware pipeline

Insert middleware into the default pipeline:

:::code language="csharp" source="~/samples/extensions/CustomMiddleware.cs" id="custom_middleware":::

### Custom health checks

Add application-specific health checks:

:::code language="csharp" source="~/samples/extensions/CustomHealthChecks.cs" id="custom_health":::

> [!WARNING]
> Custom middleware must be added after `ConfigureApiUsingDefaults()` to maintain proper ordering.

### Authentication configuration

Configure authentication requirements:

:::code language="csharp" source="~/samples/extensions/AuthConfiguration.cs" id="auth_config":::

## Performance considerations

Platform extensions optimize for production workloads:

- **Connection pooling** - Npgsql pools configured for high concurrency
- **Prepared statements** - Database commands use prepared statements when possible
- **Batched telemetry** - OpenTelemetry exports are batched to reduce overhead
- **Efficient serialization** - System.Text.Json with source generators

Typical overhead compared to manual configuration:
- Service registration: ~2ms additional startup time
- Request processing: <1% performance impact
- Memory usage: ~5MB additional for telemetry and validation

> [!NOTE]
> Performance measurements taken with .NET 9 on production-equivalent hardware.

:::moniker-end

## Additional resources

- [Platform architecture](architecture.md) - Core design principles and patterns
- [API development](api/overview.md) - Building REST and gRPC services
- [Database integration](database-integration.md) - High-performance data access
- [Messaging patterns](messaging/overview.md) - Event-driven architecture
- [Extension samples](https://github.com/operations-platform/extension-samples) - Complete usage examples