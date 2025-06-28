using Operations.ServiceDefaults.OpenTelemetry;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// <TracingSetup>
builder.AddOpenTelemetry(o =>
{
    o.Tracing.AddSource("MyService");
    o.Tracing.AddAspNetCoreInstrumentation();
});
// </TracingSetup>

var app = builder.Build();
app.Run();
