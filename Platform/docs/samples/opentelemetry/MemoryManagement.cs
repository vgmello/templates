using Operations.ServiceDefaults.OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

// <MemoryOptimization>
builder.AddOpenTelemetry(o =>
{
    o.Tracing.AddAspNetCoreInstrumentation();
    o.Tracing.SetResourceBuilder(ResourceBuilder.CreateDefault());
});
// </MemoryOptimization>

var app = builder.Build();
app.Run();
