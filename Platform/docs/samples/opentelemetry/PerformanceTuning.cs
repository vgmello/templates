using Operations.ServiceDefaults.OpenTelemetry;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.AddOpenTelemetry(o =>
{
    o.Tracing.SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(1.0)));
});

var app = builder.Build();
app.Run();
