using Operations.ServiceDefaults.OpenTelemetry;
using OpenTelemetry.Exporter;

var builder = WebApplication.CreateBuilder(args);

// <OtlpConfiguration>
builder.AddOpenTelemetry(o =>
{
    o.Tracing.AddOtlpExporter();
});
// </OtlpConfiguration>

var app = builder.Build();
app.Run();
