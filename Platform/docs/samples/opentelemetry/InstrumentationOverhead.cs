using Operations.ServiceDefaults.OpenTelemetry;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// <OverheadMinimization>
builder.AddOpenTelemetry(o =>
{
    o.Tracing.AddSource("CriticalPath");
    o.Tracing.SetSampler(new AlwaysOnSampler());
});
// </OverheadMinimization>

var app = builder.Build();
app.Run();
