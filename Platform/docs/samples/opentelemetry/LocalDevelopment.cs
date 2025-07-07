using Operations.ServiceDefaults.OpenTelemetry;
using OpenTelemetry.Exporter;

var builder = WebApplication.CreateBuilder(args);

// <DevSetup>
builder.AddOpenTelemetry(o =>
{
    o.Tracing.AddConsoleExporter();
});
// </DevSetup>

var app = builder.Build();
app.Run();
