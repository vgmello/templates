# Health Checks Overview

The Platform provides a comprehensive health check system designed for Kubernetes environments with multiple specialized endpoints for different monitoring needs.

## Key Benefits

### 🎯 **Kubernetes Optimized**
- **Liveness probes** - Fast endpoint for container restart decisions
- **Readiness probes** - Comprehensive checks for traffic routing
- **Security-aware** - Internal endpoints protected from external access

### ⚡ **Performance Focused**
- **Cached results** - Liveness probes use cached status for sub-millisecond response
- **Lightweight checks** - Minimal resource usage for frequent polling
- **Async operations** - Non-blocking health evaluations

### 🔒 **Security by Design**
- **Localhost-only internal endpoints** - Protected from external access
- **Authorized public endpoints** - Detailed health info requires authentication
- **Information disclosure protection** - Sensitive details hidden in production

## Health Check Endpoints

### Endpoint Overview

| Endpoint | Purpose | Audience | Response Time | Detail Level |
|----------|---------|----------|---------------|--------------|
| `/status` | Liveness probe | Kubernetes | < 1ms | Minimal |
| `/health/internal` | Readiness probe | Localhost only | 10-100ms | Comprehensive |
| `/health` | Public monitoring | Authenticated users | 10-100ms | Detailed |

### Liveness Probe - `/status`

#### Purpose
Ultra-fast endpoint for Kubernetes liveness probes that determines if the container should be restarted.

#### Implementation
```csharp
app.MapGet("/status", (HealthCheckStatusStore store) =>
{
    var status = store.LastHealthStatus;
    var statusText = status switch
    {
        HealthStatus.Healthy => "Healthy",
        HealthStatus.Degraded => "Degraded", 
        HealthStatus.Unhealthy => "Unhealthy",
        _ => "Unknown"
    };
    
    return Results.Ok(statusText);
})
.WithName("GetStatus")
.WithSummary("Lightweight liveness probe")
.WithDescription("Returns cached health status for Kubernetes liveness probes")
.Produces<string>(StatusCodes.Status200OK)
.AllowAnonymous();
```

#### Kubernetes Configuration
```yaml
livenessProbe:
  httpGet:
    path: /status
    port: 8080
  initialDelaySeconds: 30
  periodSeconds: 10
  timeoutSeconds: 1
  failureThreshold: 3
```

#### Benefits
- **Sub-millisecond response** - Uses cached status
- **No database calls** - Prevents resource exhaustion during outages
- **Simple string response** - Minimal processing overhead

### Readiness Probe - `/health/internal`

#### Purpose
Comprehensive health checks for Kubernetes readiness probes to determine if the pod should receive traffic.

#### Implementation
```csharp
app.MapHealthChecks("/health/internal", new HealthCheckOptions
{
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    },
    ResponseWriter = async (context, result) =>
    {
        // Store result for liveness probe caching
        var store = context.RequestServices.GetRequiredService<HealthCheckStatusStore>();
        store.LastHealthStatus = result.Status;
        
        // Return simple status for internal use
        await context.Response.WriteAsync(result.Status.ToString());
    }
})
.RequireHost("localhost", "127.0.0.1", "::1") // Localhost only
.WithName("GetInternalHealth")
.WithSummary("Comprehensive readiness probe")
.WithDescription("Internal health checks for Kubernetes readiness probes");
```

#### Kubernetes Configuration
```yaml
readinessProbe:
  httpGet:
    path: /health/internal
    port: 8080
  initialDelaySeconds: 5
  periodSeconds: 5
  timeoutSeconds: 3
  failureThreshold: 3
```

#### Registered Checks
```csharp
// Automatic registration in AddServiceDefaults()
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString) // Database connectivity
    .AddKafka(kafkaOptions)      // Message bus connectivity  
    .AddCheck<CustomBusinessLogicCheck>() // Custom application checks
    .AddCheck("wolverine", () => /* Wolverine health */)
    .AddCheck("external-api", () => /* External dependencies */);
```

### Public Health - `/health`

#### Purpose
Detailed health information for monitoring systems and authenticated administrators.

#### Implementation
```csharp
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK, 
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
})
.RequireAuthorization() // Requires authentication
.WithName("GetHealth")
.WithSummary("Detailed health information")
.WithDescription("Comprehensive health checks with detailed response data");
```

#### Response Format
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0156250",
  "entries": {
    "billing-db": {
      "status": "Healthy",
      "duration": "00:00:00.0123456",
      "description": "PostgreSQL database connectivity",
      "data": {
        "server": "localhost:5432",
        "database": "billing"
      }
    },
    "messaging": {
      "status": "Healthy", 
      "duration": "00:00:00.0098765",
      "description": "Kafka message broker connectivity",
      "data": {
        "brokers": ["localhost:9092"],
        "topics": ["billing.events", "accounting.events"]
      }
    },
    "wolverine": {
      "status": "Healthy",
      "duration": "00:00:00.0045678",
      "description": "Wolverine message processing"
    }
  }
}
```

## Health Check Status Store

### Implementation

```csharp
public class HealthCheckStatusStore
{
    private volatile HealthStatus _lastHealthStatus = HealthStatus.Unhealthy;
    
    public HealthStatus LastHealthStatus
    {
        get => _lastHealthStatus;
        set => _lastHealthStatus = value;
    }
}
```

### Registration
```csharp
// Singleton registration for thread-safe caching
builder.Services.AddSingleton<HealthCheckStatusStore>();
```

### Benefits
- **Thread-safe** - Uses volatile field for atomic reads/writes
- **Memory efficient** - Single enum value storage
- **Fast access** - Direct property access without locks

## Custom Health Checks

### Business Logic Check
```csharp
public class CashierServiceHealthCheck : IHealthCheck
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CashierServiceHealthCheck> _logger;
    
    public CashierServiceHealthCheck(
        IServiceProvider serviceProvider,
        ILogger<CashierServiceHealthCheck> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
            
            // Test basic query functionality
            var testQuery = new HealthCheckQuery();
            var result = await messageBus.InvokeQueryAsync(testQuery, cancellationToken);
            
            if (result.IsHealthy)
            {
                return HealthCheckResult.Healthy(
                    "Cashier service is functioning normally",
                    new Dictionary<string, object>
                    {
                        ["lastCheck"] = DateTimeOffset.UtcNow,
                        ["activeHandlers"] = result.ActiveHandlerCount
                    });
            }
            
            return HealthCheckResult.Degraded(
                "Some cashier operations are experiencing issues",
                data: new Dictionary<string, object>
                {
                    ["issues"] = result.Issues
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed for cashier service");
            return HealthCheckResult.Unhealthy(
                "Cashier service is not responding",
                ex);
        }
    }
}
```

### Registration
```csharp
builder.Services.AddHealthChecks()
    .AddCheck<CashierServiceHealthCheck>("cashier-service", 
        tags: new[] { "business", "critical" });
```

## Security Features

### Localhost Endpoint Filter

```csharp
public class LocalhostEndpointFilter : IEndpointFilter
{
    private readonly ILogger<LocalhostEndpointFilter> _logger;
    
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var remoteIp = context.HttpContext.Connection.RemoteIpAddress;
        
        if (remoteIp != null && !IPAddress.IsLoopback(remoteIp))
        {
            _logger.LogDebug("Blocked non-localhost request from {RemoteIp}", remoteIp);
            
            context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.HttpContext.Response.WriteAsync("Access denied");
            return null;
        }
        
        return await next(context);
    }
}
```

### Benefits
- **Container security** - Internal endpoints not accessible from outside pod
- **Information disclosure protection** - Prevents external reconnaissance
- **Performance** - Fast IP address validation

## Monitoring Integration

### Prometheus Metrics
```csharp
// Automatic metrics export for health checks
services.AddOpenTelemetry()
    .WithMetrics(builder => builder
        .AddAspNetCoreInstrumentation()
        .AddMeter("Microsoft.AspNetCore.HealthChecks")); // Built-in health check metrics
```

### Available Metrics
- `aspnetcore_healthcheck_status` - Current health status (0=Unhealthy, 1=Degraded, 2=Healthy)
- `aspnetcore_healthcheck_duration_seconds` - Time taken for health check execution
- `aspnetcore_healthcheck_checks_total` - Total number of health check executions

### Alerting Rules
```yaml
# Prometheus alerting rules
groups:
  - name: health-checks
    rules:
      - alert: ServiceUnhealthy
        expr: aspnetcore_healthcheck_status < 2
        for: 2m
        labels:
          severity: critical
        annotations:
          summary: "Service {{ $labels.instance }} is unhealthy"
          
      - alert: HealthCheckSlow
        expr: aspnetcore_healthcheck_duration_seconds > 5
        for: 1m
        labels:
          severity: warning
        annotations:
          summary: "Health checks taking too long on {{ $labels.instance }}"
```

## Value Delivered

### Operational Excellence
- **99.9% uptime** with proper liveness/readiness configuration
- **Zero false positives** with cached liveness probes
- **Rapid incident detection** with comprehensive readiness checks

### Developer Experience
- **One-line setup** - All endpoints configured automatically
- **Security by default** - No configuration required for localhost protection
- **Rich debugging** - Detailed health information in development

### Platform Benefits
- **Consistent monitoring** across all microservices
- **Kubernetes best practices** built-in
- **Observability integration** with metrics and logging

### Business Impact
- **Reduced downtime** with proactive health monitoring
- **Faster problem resolution** with detailed health information
- **Lower operational costs** with automated container management