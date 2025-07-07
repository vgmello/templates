using Operations.ServiceDefaults.OpenTelemetry;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

// <MetricsSetup>
builder.AddOpenTelemetry(o =>
{
    o.Metrics.AddAspNetCoreInstrumentation();
    o.Metrics.AddMeter("MyService.Metrics");
});
// </MetricsSetup>

var app = builder.Build();
app.Run();
