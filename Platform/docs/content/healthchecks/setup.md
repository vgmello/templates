# Health Checks Setup and Configuration

This guide covers the setup and configuration of health checks in the Operations platform.

## Overview

The Operations platform provides comprehensive health check functionality through the `Operations.ServiceDefaults.HealthChecks` package, enabling monitoring of application health, dependencies, and system resources.

## Basic Setup

### Service Registration

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add service defaults (includes health checks)
builder.AddServiceDefaults();

var app = builder.Build();

// Map health check endpoints
app.MapDefaultEndpoints();

app.Run();
```

### Manual Health Check Registration

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddNpgSql(connectionString, name: "postgres")
    .AddUrlGroup(new Uri("https://api.external.com/health"), "external-api");
```

## Built-in Health Checks

### Database Health Checks

```csharp
// PostgreSQL
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "database");

// Multiple databases
builder.Services.AddHealthChecks()
    .AddNpgSql(billingConnectionString, name: "billing-db")
    .AddNpgSql(serviceBusConnectionString, name: "servicebus-db");
```

### HTTP Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddUrlGroup(new Uri("https://api.dependency.com/health"), "dependency-api")
    .AddHttpHealthCheck("external-service", "https://service.com/ping");
```

### Custom Health Checks

```csharp
public class CashierServiceHealthCheck : IHealthCheck
{
    private readonly ICashierRepository _repository;

    public CashierServiceHealthCheck(ICashierRepository repository)
    {
        _repository = repository;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var count = await _repository.GetActiveCashierCountAsync(cancellationToken);
            
            return count > 0 
                ? HealthCheckResult.Healthy($"Active cashiers: {count}")
                : HealthCheckResult.Degraded("No active cashiers");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Cashier service unavailable", ex);
        }
    }
}

// Registration
builder.Services.AddHealthChecks()
    .AddCheck<CashierServiceHealthCheck>("cashier-service");
```

## Health Check Endpoints

### Default Endpoints

The platform exposes several health check endpoints:

- `/health` - Overall health status
- `/health/ready` - Readiness probe
- `/health/live` - Liveness probe

### Custom Endpoint Configuration

```csharp
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
```

## Health Check Tags

Organize health checks using tags:

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("database", () => HealthCheckResult.Healthy(), tags: new[] { "ready", "db" })
    .AddCheck("external-api", () => HealthCheckResult.Healthy(), tags: new[] { "ready", "external" })
    .AddCheck("memory", () => HealthCheckResult.Healthy(), tags: new[] { "live" });
```

## Configuration Options

### appsettings.json

```json
{
  "HealthChecks": {
    "Timeout": "00:00:30",
    "CheckInterval": "00:01:00",
    "FailureStatus": "Unhealthy",
    "DetailedErrors": true
  }
}
```

### Environment-Specific Configuration

```json
{
  "HealthChecks": {
    "Database": {
      "Enabled": true,
      "ConnectionString": "Host=localhost;Database=billing;Username=postgres"
    },
    "ExternalServices": {
      "Enabled": false
    }
  }
}
```

## Health Check Results

### Status Levels

- **Healthy**: Service is operating normally
- **Degraded**: Service is operational but with reduced functionality
- **Unhealthy**: Service is not operational

### Response Format

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0123456",
  "entries": {
    "database": {
      "status": "Healthy",
      "duration": "00:00:00.0056789",
      "data": {
        "connection": "active"
      }
    },
    "external-api": {
      "status": "Degraded",
      "duration": "00:00:00.0234567",
      "description": "High response time",
      "data": {
        "responseTime": "2.5s"
      }
    }
  }
}
```

## Integration with Kubernetes

### Liveness Probe

```yaml
livenessProbe:
  httpGet:
    path: /health/live
    port: 8080
  initialDelaySeconds: 30
  periodSeconds: 10
```

### Readiness Probe

```yaml
readinessProbe:
  httpGet:
    path: /health/ready
    port: 8080
  initialDelaySeconds: 5
  periodSeconds: 5
```

## Monitoring and Alerting

### Health Check Status Store

The platform includes a health check status store for tracking health over time:

```csharp
builder.Services.AddSingleton<HealthCheckStatusStore>();

// Access health history
app.MapGet("/health/history", (HealthCheckStatusStore store) => 
    store.GetHealthCheckHistory());
```

### Integration with OpenTelemetry

Health check results are automatically exported as metrics:

```csharp
// Custom health metrics
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddMeter("HealthChecks"));
```

## Best Practices

1. **Granular Checks**: Create specific health checks for each dependency
2. **Timeout Configuration**: Set appropriate timeouts for external dependencies
3. **Graceful Degradation**: Use Degraded status for non-critical issues
4. **Avoid Cascading Failures**: Don't let health checks impact service performance
5. **Meaningful Data**: Include relevant diagnostic information in health check responses

## Troubleshooting

### Common Issues

- **Timeout errors**: Increase health check timeout values
- **False positives**: Review health check logic and thresholds
- **Performance impact**: Optimize health check queries and reduce frequency
- **Dependency failures**: Implement circuit breaker patterns

### Debugging

Enable health check logging:

```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.Extensions.Diagnostics.HealthChecks": "Debug"
    }
  }
}
```

## See Also

- [Health Checks Overview](overview.md)
- [OpenTelemetry Setup](../opentelemetry/setup.md)
- [Service Defaults](../architecture/service-defaults.md)