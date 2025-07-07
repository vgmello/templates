using Operations.ServiceDefaults;
using Operations.ServiceDefaults.Api;
using Operations.ServiceDefaults.Logging;
using Operations.ServiceDefaults.OpenTelemetry;
using Operations.ServiceDefaults.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// <ServiceDefaultsConfiguration>
// Add Service Defaults - this configures cross-cutting concerns
// including logging, health checks, OpenTelemetry, and metrics
builder.AddServiceDefaults();

// For API services, add additional API-specific defaults
// including OpenAPI documentation, problem details, and endpoint filters
builder.AddApiServiceDefaults();

// Service Defaults includes:
// - Structured logging with Serilog
// - Health checks with multiple endpoints
// - OpenTelemetry tracing and metrics
// - Metrics collection and export
// - Default HTTP client resilience policies
// - Service discovery integration
// </ServiceDefaultsConfiguration>

// <CustomConfiguration>
// You can customize individual aspects of Service Defaults
builder.Services.Configure<LoggingSetupOptions>(options =>
{
    options.EnableConsoleLogging = true;
    options.MinimumLevel = LogLevel.Information;
});

builder.Services.Configure<OpenTelemetrySetupOptions>(options =>
{
    options.ServiceName = "MyCustomService";
    options.ServiceVersion = "1.2.3";
    options.EnableTracing = true;
    options.EnableMetrics = true;
});
// </CustomConfiguration>

// <ApplicationServices>
// Add your application-specific services after Service Defaults
builder.Services.AddControllers();
builder.Services.AddScoped<IMyService, MyService>();
// </ApplicationServices>

var app = builder.Build();

// <DefaultMiddleware>
// Service Defaults automatically configures essential middleware
// including health check endpoints, metrics collection, and request logging

// Map default endpoints (health checks, metrics, etc.)
app.MapDefaultEndpoints();

// Add your application-specific middleware and routes
app.MapControllers();
// </DefaultMiddleware>

app.Run();

// <ServiceInterfaces>
public interface IMyService
{
    Task<string> GetDataAsync(CancellationToken cancellationToken = default);
}

public class MyService : IMyService
{
    private readonly ILogger<MyService> _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public async Task<string> GetDataAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting data from service");
        
        // Simulate async work
        await Task.Delay(100, cancellationToken);
        
        return "Service data";
    }
}
// </ServiceInterfaces>

// <ConfigurationOptions>
public class LoggingSetupOptions
{
    public bool EnableConsoleLogging { get; set; } = true;
    public LogLevel MinimumLevel { get; set; } = LogLevel.Information;
}

public class OpenTelemetrySetupOptions
{
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceVersion { get; set; } = "1.0.0";
    public bool EnableTracing { get; set; } = true;
    public bool EnableMetrics { get; set; } = true;
}
// </ConfigurationOptions>