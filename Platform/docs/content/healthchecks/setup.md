---
title: Health Check Setup Extensions
description: Learn how to configure and map health check endpoints in your application using the provided setup extensions.
---

# Health Check Setup Extensions

The `HealthCheckSetupExtensions` class provides a set of extension methods to easily configure and map health check endpoints within your ASP.NET Core application. These extensions simplify the process of exposing application health status for monitoring and orchestration systems.

## Key Features

### MapDefaultHealthCheckEndpoints

This extension method configures and maps a set of default health check endpoints. It provides different endpoints for various monitoring needs, including liveness probes, readiness probes, and detailed public health status.

#### Configured Endpoints

-   **`/status`**: A lightweight liveness probe. This endpoint returns the string representation of the last recorded health status (e.g., "Healthy", "Unhealthy"). It does *not* execute health checks itself but reflects the last known state. This is ideal for quick checks by orchestrators to determine if the application process is alive.

-   **`/health/internal`**: A container-only readiness probe. This endpoint provides simplified health status information and is primarily intended for use within containerized environments (e.g., Kubernetes readiness probes). When run locally, it provides the same detailed output as the `/health` endpoint. It is restricted to localhost and includes a `LocalhostEndpointFilter`.

-   **`/health`**: A public, detailed health probe. This endpoint returns comprehensive health status information, including the status of all registered health checks, their durations, and any associated error messages. It requires authorization, making it suitable for exposing detailed health information to authorized consumers.

#### Usage example

Call `MapDefaultHealthCheckEndpoints` on your `WebApplication` instance:

```csharp
// In Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add health checks (e.g., for database, external services)
builder.Services.AddHealthChecks();

var app = builder.Build();

// Map the default health check endpoints
app.MapDefaultHealthCheckEndpoints();

app.Run();
```

## Health Check Logging

The extensions also include logging for health check responses. Successful health checks are logged at a debug level, while unhealthy or degraded statuses are logged at error or warning levels, respectively, providing insights into the health of your application's components.

## Customizing Health Checks

You can add custom health checks to your application using the standard ASP.NET Core `AddHealthChecks()` method and then chaining additional health check registrations. These custom checks will be included in the detailed `/health` and `/health/internal` reports.

```csharp
// In Program.cs
builder.Services.AddHealthChecks()
    .AddSqlServer("ConnectionStrings:DefaultConnection", name: "SQL Server");
```

## See also

- [Health Check Status Store](./health-check-status-store.md)
- [Endpoint Filters](../api/endpoint-filters.md)
