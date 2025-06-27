using Operations.ServiceDefaults;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults first
builder.AddServiceDefaults();

// #region CustomLogging
// Override specific logging settings after calling AddServiceDefaults
builder.Host.UseSerilog((context, config) =>
{
    config.WriteTo.Console()
          .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
          .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning);
});
// #endregion

// #region CustomTelemetry
// Add custom tracing sources and metrics
builder.Services.ConfigureOpenTelemetryTracerProvider(tracing =>
{
    tracing.AddSource("MyApplication");
});

builder.Services.ConfigureOpenTelemetryMeterProvider(metrics =>
{
    metrics.AddMeter("MyApplication.Metrics");
});
// #endregion

var app = builder.Build();

app.MapDefaultEndpoints();
await app.RunAsync(args);