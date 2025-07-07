using Operations.ServiceDefaults.OpenTelemetry;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// <DatabaseTracing>
builder.AddOpenTelemetry(o =>
{
    o.Tracing.AddNpgsql();
});
// </DatabaseTracing>

var app = builder.Build();
app.Run();
