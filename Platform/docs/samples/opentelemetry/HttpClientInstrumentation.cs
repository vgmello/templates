using Operations.ServiceDefaults.OpenTelemetry;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// <HttpClientTracing>
builder.AddOpenTelemetry(o =>
{
    o.Tracing.AddHttpClientInstrumentation();
});
// </HttpClientTracing>

var app = builder.Build();
app.Run();
