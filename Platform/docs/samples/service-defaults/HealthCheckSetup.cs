using Operations.ServiceDefaults;
using Operations.ServiceDefaults.Api;
using Operations.ServiceDefaults.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// <HealthCheckConfiguration>
// Service Defaults automatically configures health checks with sensible defaults
builder.AddServiceDefaults();
builder.AddApiServiceDefaults();

// Customize health check behavior
builder.Services.Configure<HealthCheckSetupOptions>(options =>
{
    // Configure default timeouts for all health checks
    options.DefaultTimeout = TimeSpan.FromSeconds(10);
    
    // Configure failure threshold before marking as unhealthy
    options.FailureThreshold = 3;
    
    // Configure delay between health check executions
    options.Period = TimeSpan.FromSeconds(30);
    
    // Configure startup delay before first health check
    options.Delay = TimeSpan.FromSeconds(5);
});

// Add custom health checks with Service Defaults integration
builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "database",
        tags: new[] { "critical", "database" })
    .AddRedis(
        redisConnectionString: builder.Configuration.GetConnectionString("Redis")!,
        name: "cache",
        tags: new[] { "optional", "cache" });
// </HealthCheckConfiguration>

var app = builder.Build();

// <HealthCheckEndpoints>
// Service Defaults provides three health check endpoints:
// - /status - Fast liveness probe (cached results)
// - /health/internal - Readiness probe (localhost only)
// - /health - Detailed health status (authenticated)

app.MapDefaultEndpoints();
// </HealthCheckEndpoints>

app.Run();

// <HealthCheckSetupOptions>
public class HealthCheckSetupOptions
{
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public int FailureThreshold { get; set; } = 3;
    public TimeSpan Period { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan Delay { get; set; } = TimeSpan.Zero;
}
// </HealthCheckSetupOptions>