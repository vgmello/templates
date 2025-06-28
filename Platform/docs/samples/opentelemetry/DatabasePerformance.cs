using Operations.ServiceDefaults.OpenTelemetry;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// <DatabaseAnalysis>
builder.AddOpenTelemetry(o =>
{
    o.Tracing.AddNpgsql();
});
// </DatabaseAnalysis>

var app = builder.Build();
app.Run();
