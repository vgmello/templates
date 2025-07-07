using Operations.ServiceDefaults.OpenTelemetry;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// <TestingInstrumentation>
builder.AddOpenTelemetry(o =>
{
    o.Tracing.AddInMemoryExporter(_ => { });
});
// </TestingInstrumentation>

var app = builder.Build();
app.Run();
