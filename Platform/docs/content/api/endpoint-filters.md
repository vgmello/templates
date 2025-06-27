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

```csharp
public class LocalhostEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, 
        EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        
        // Allow localhost access in development
        if (httpContext.Request.IsLocal() || 
            !httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsProduction())
        {
            return await next(context);
        }

        // Block in production
        httpContext.Response.StatusCode = 403;
        return Results.Forbid();
    }
}
```

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
public class ValidationEndpointFilter<T> : IEndpointFilter where T : class
{
    private readonly IValidator<T> _validator;

    public ValidationEndpointFilter(IValidator<T> validator)
    {
        _validator = validator;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, 
        EndpointFilterDelegate next)
    {
        // Find the parameter of type T
        var argument = context.Arguments
            .OfType<T>()
            .FirstOrDefault();

        if (argument is not null)
        {
            var validationResult = await _validator.ValidateAsync(argument);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }
        }

        return await next(context);
    }
}
```

### Authentication Filter

```csharp
public class RequireRoleEndpointFilter : IEndpointFilter
{
    private readonly string _requiredRole;

    public RequireRoleEndpointFilter(string requiredRole)
    {
        _requiredRole = requiredRole;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, 
        EndpointFilterDelegate next)
    {
        var user = context.HttpContext.User;
        
        if (!user.Identity?.IsAuthenticated == true)
        {
            return Results.Unauthorized();
        }

        if (!user.IsInRole(_requiredRole))
        {
            return Results.Forbid();
        }

        return await next(context);
    }
}
```

### Rate Limiting Filter

```csharp
public class RateLimitEndpointFilter : IEndpointFilter
{
    private readonly IMemoryCache _cache;
    private readonly int _maxRequests;
    private readonly TimeSpan _window;

    public RateLimitEndpointFilter(
        IMemoryCache cache, 
        int maxRequests = 100, 
        TimeSpan? window = null)
    {
        _cache = cache;
        _maxRequests = maxRequests;
        _window = window ?? TimeSpan.FromMinutes(1);
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, 
        EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var clientId = GetClientIdentifier(httpContext);
        var key = $"rate_limit:{clientId}";

        var requestCount = _cache.Get<int>(key);
        
        if (requestCount >= _maxRequests)
        {
            httpContext.Response.Headers.Add("Retry-After", 
                _window.TotalSeconds.ToString(CultureInfo.InvariantCulture));
            
            return Results.Problem(
                statusCode: 429,
                title: "Too Many Requests",
                detail: $"Rate limit exceeded. Maximum {_maxRequests} requests per {_window}.");
        }

        _cache.Set(key, requestCount + 1, _window);
        
        return await next(context);
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Use user ID if authenticated, otherwise use IP address
        return context.User.Identity?.IsAuthenticated == true
            ? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous"
            : context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
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
public class ConditionalLoggingFilter : IEndpointFilter
{
    private readonly ILogger<ConditionalLoggingFilter> _logger;
    private readonly IConfiguration _configuration;

    public ConditionalLoggingFilter(
        ILogger<ConditionalLoggingFilter> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, 
        EndpointFilterDelegate next)
    {
        var enableLogging = _configuration.GetValue<bool>("Features:DetailedLogging");
        
        if (enableLogging)
        {
            _logger.LogInformation("Executing endpoint with detailed logging");
        }

        return await next(context);
    }
}
```

### Filter with Dependencies

```csharp
public class CacheEndpointFilter : IEndpointFilter
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheEndpointFilter> _logger;

    public CacheEndpointFilter(
        IDistributedCache cache,
        ILogger<CacheEndpointFilter> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, 
        EndpointFilterDelegate next)
    {
        var request = context.HttpContext.Request;
        
        // Only cache GET requests
        if (!HttpMethods.IsGet(request.Method))
        {
            return await next(context);
        }

        var cacheKey = GenerateCacheKey(request);
        var cachedResponse = await _cache.GetStringAsync(cacheKey);
        
        if (cachedResponse != null)
        {
            _logger.LogInformation("Cache hit for key: {CacheKey}", cacheKey);
            return Results.Json(JsonSerializer.Deserialize<object>(cachedResponse));
        }

        var result = await next(context);
        
        // Cache successful responses
        if (result is IResult okResult)
        {
            var serialized = JsonSerializer.Serialize(result);
            await _cache.SetStringAsync(cacheKey, serialized, TimeSpan.FromMinutes(15));
            _logger.LogInformation("Cached response for key: {CacheKey}", cacheKey);
        }

        return result;
    }

    private static string GenerateCacheKey(HttpRequest request)
    {
        return $"endpoint_cache:{request.Path}{request.QueryString}";
    }
}
```

### Async Filter with Cancellation

```csharp
public class TimeoutEndpointFilter : IEndpointFilter
{
    private readonly TimeSpan _timeout;

    public TimeoutEndpointFilter(TimeSpan timeout)
    {
        _timeout = timeout;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, 
        EndpointFilterDelegate next)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(
            context.HttpContext.RequestAborted);
        
        cts.CancelAfter(_timeout);

        try
        {
            return await next(context);
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
        {
            return Results.Problem(
                statusCode: 408,
                title: "Request Timeout",
                detail: $"Request exceeded the maximum allowed time of {_timeout}.");
        }
    }
}
```

## Filter Order and Pipeline

### Execution Order

Filters execute in the order they are added:

```csharp
app.MapPost("/cashiers", CreateCashier)
    .AddEndpointFilter<LoggingEndpointFilter>()        // Executes first
    .AddEndpointFilter<ValidationEndpointFilter<CreateCashierCommand>>() // Executes second
    .AddEndpointFilter<RateLimitEndpointFilter>();     // Executes last
```

### Short-Circuiting

Filters can short-circuit the pipeline:

```csharp
public class ShortCircuitFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, 
        EndpointFilterDelegate next)
    {
        // Condition to short-circuit
        if (ShouldShortCircuit())
        {
            return Results.BadRequest("Request rejected by filter");
        }

        // Continue pipeline
        return await next(context);
    }
}
```

## Testing Endpoint Filters

### Unit Testing

```csharp
[Fact]
public async Task ValidationFilter_InvalidModel_ReturnsValidationProblem()
{
    // Arrange
    var validator = new Mock<IValidator<CreateCashierCommand>>();
    validator.Setup(v => v.ValidateAsync(It.IsAny<CreateCashierCommand>(), default))
        .ReturnsAsync(new ValidationResult(new[] 
        { 
            new ValidationFailure("Name", "Name is required") 
        }));

    var filter = new ValidationEndpointFilter<CreateCashierCommand>(validator.Object);
    var context = CreateMockContext(new CreateCashierCommand());

    // Act
    var result = await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>(Results.Ok()));

    // Assert
    Assert.IsType<ProblemHttpResult>(result);
}
```

### Integration Testing

```csharp
[Fact]
public async Task CashierEndpoint_WithFilters_WorksCorrectly()
{
    // Arrange
    using var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();

    // Act
    var response = await client.PostAsJsonAsync("/cashiers", new CreateCashierCommand
    {
        Name = "Test Cashier",
        Email = "test@example.com",
        Currencies = new[] { "USD" }
    });

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
}
```

## Best Practices

1. **Single Responsibility**: Keep filters focused on specific concerns
2. **Performance**: Minimize overhead in frequently executed filters
3. **Error Handling**: Handle exceptions gracefully
4. **Dependency Injection**: Use DI for filter dependencies
5. **Testing**: Write comprehensive tests for custom filters
6. **Documentation**: Document filter behavior and usage
7. **Ordering**: Consider filter execution order carefully

## See Also

- [Service Defaults](../../architecture/service-defaults.md)
- [OpenAPI Documentation](../openapi/overview.md)
- [gRPC Integration](grpc.md)
