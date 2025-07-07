using Operations.ServiceDefaults.OpenTelemetry;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// <ServiceCommunication>
builder.AddOpenTelemetry(o =>
{
    o.Tracing.AddSource("ServiceA");
    o.Tracing.AddHttpClientInstrumentation();
});
// </ServiceCommunication>

var app = builder.Build();
app.Run();
