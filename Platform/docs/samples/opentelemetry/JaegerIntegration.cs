using Operations.ServiceDefaults.OpenTelemetry;
using OpenTelemetry.Exporter;

var builder = WebApplication.CreateBuilder(args);

// <JaegerExport>
builder.AddOpenTelemetry(o =>
{
    o.Tracing.AddJaegerExporter();
});
// </JaegerExport>

var app = builder.Build();
app.Run();
