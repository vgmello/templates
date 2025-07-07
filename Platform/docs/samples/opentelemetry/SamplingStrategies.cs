using Operations.ServiceDefaults.OpenTelemetry;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// <SamplingConfig>
builder.AddOpenTelemetry(o =>
{
    o.Tracing.SetSampler(new TraceIdRatioBasedSampler(0.5));
});
// </SamplingConfig>

var app = builder.Build();
app.Run();
