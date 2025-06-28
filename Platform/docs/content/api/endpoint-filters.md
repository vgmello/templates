---
title: Endpoint Filters
description: Learn about endpoint filters in the Operations platform, including built-in filters and creating custom filters.
---

# Endpoint Filters

This guide covers endpoint filters in the Operations platform, including built-in filters and creating custom filters.

## Overview

Endpoint filters in the Operations platform provide a way to execute code before and after minimal API endpoints. They offer a lightweight alternative to middleware for endpoint-specific logic.

## Built-in Filters

### LocalhostEndpointFilter

The platform includes a `LocalhostEndpointFilter` that restricts access to certain endpoints when running in production:

[!code-csharp[](~/samples/api/endpoint-filters/LocalhostEndpointFilter.cs)]

### Usage with Service Defaults

The service defaults automatically apply appropriate filters:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Automatically includes endpoint filters
builder.AddServiceDefaults();

var app = builder.Build();

// Development-only endpoint with localhost filter
app.MapGet("/debug/config", () => builder.Configuration.AsEnumerable())
    .AddEndpointFilter<LocalhostEndpointFilter>();
```

## Creating Custom Filters

### Basic Endpoint Filter

```csharp
public class LoggingEndpointFilter : IEndpointFilter
{
    private readonly ILogger<LoggingEndpointFilter> _logger;

    public LoggingEndpointFilter(ILogger<LoggingEndpointFilter> logger)
    {
        _logger = logger;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, 
        EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var endpoint = httpContext.GetEndpoint()?.DisplayName;
        
        _logger.LogInformation("Executing endpoint: {Endpoint}", endpoint);
        
        var stopwatch = Stopwatch.StartNew();
        var result = await next(context);
        stopwatch.Stop();
        
        _logger.LogInformation("Endpoint {Endpoint} completed in {ElapsedMs}ms", 
            endpoint, stopwatch.ElapsedMilliseconds);
        
        return result;
    }
}
```

### Validation Filter

```csharp
[!code-csharp[](~/samples/api/endpoint-filters/ValidationEndpointFilter.cs)]
```

### Authentication Filter

[!code-csharp[](~/samples/api/endpoint-filters/RequireRoleEndpointFilter.cs)]

### Rate Limiting Filter

```csharp
[!code-csharp[](~/samples/api/endpoint-filters/RateLimitEndpointFilter.cs)]
```

## Filter Registration

### Global Registration

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register filters with DI
builder.Services.AddScoped<LoggingEndpointFilter>();
builder.Services.AddScoped<ValidationEndpointFilter<CreateCashierCommand>>();
builder.Services.AddSingleton<IMemoryCache, MemoryCache>();

var app = builder.Build();
```

### Endpoint-Specific Application

```csharp
// Apply single filter
app.MapPost("/cashiers", CreateCashier)
    .AddEndpointFilter<ValidationEndpointFilter<CreateCashierCommand>>();

// Apply multiple filters
app.MapGet("/admin/health", GetDetailedHealth)
    .AddEndpointFilter<LocalhostEndpointFilter>()
    .AddEndpointFilter<LoggingEndpointFilter>();

// Apply filter with parameters
app.MapPost("/payments", ProcessPayment)
    .AddEndpointFilter(new RequireRoleEndpointFilter("payment-processor"));

// Apply filter factory
app.MapGet("/limited", GetData)
    .AddEndpointFilter<RateLimitEndpointFilter>();
```

### Group-Level Filters

```csharp
var cashierGroup = app.MapGroup("/cashiers")
    .AddEndpointFilter<LoggingEndpointFilter>()
    .RequireAuthorization();

cashierGroup.MapPost("", CreateCashier)
    .AddEndpointFilter<ValidationEndpointFilter<CreateCashierCommand>>();

cashierGroup.MapGet("{id}", GetCashier);

cashierGroup.MapPut("{id}", UpdateCashier)
    .AddEndpointFilter<ValidationEndpointFilter<UpdateCashierCommand>>();
```

## Advanced Patterns

### Conditional Filters

```csharp
[!code-csharp[](~/samples/api/endpoint-filters/ConditionalLoggingFilter.cs)]
```

### Filter with Dependencies

[!code-csharp[](~/samples/api/endpoint-filters/CacheEndpointFilter.cs)]

### Async Filter with Cancellation

[!code-csharp[](~/samples/api/endpoint-filters/TimeoutEndpointFilter.cs)]

## Filter Order and Pipeline

### Execution Order

Filters execute in the order they are added:

[!code-csharp[](~/samples/api/endpoint-filters/ExecutionOrder.cs)]

### Short-Circuiting

Filters can short-circuit the pipeline:

[!code-csharp[](~/samples/api/endpoint-filters/ShortCircuitFilter.cs)]

## Testing Endpoint Filters

### Unit Testing

[!code-csharp[](~/samples/api/endpoint-filters/ValidationFilter_InvalidModel_ReturnsValidationProblem.cs)]

### Integration Testing

[!code-csharp[](~/samples/api/endpoint-filters/CashierEndpoint_WithFilters_WorksCorrectly.cs)]

## Best Practices

1. **Single Responsibility**: Keep filters focused on specific concerns
2. **Performance**: Minimize overhead in frequently executed filters
3. **Error Handling**: Handle exceptions gracefully
4. **Dependency Injection**: Use DI for filter dependencies
5. **Testing**: Write comprehensive tests for custom filters
6. **Documentation**: Document filter behavior and usage
7. **Ordering**: Consider filter execution order carefully

## See Also

- [Service Defaults](../../content/architecture/service-defaults.md)
- [[OpenAPI Documentation](openapi/overview.md)
- [gRPC Integration](grpc.md)
