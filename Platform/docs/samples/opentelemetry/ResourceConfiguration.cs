using Operations.ServiceDefaults.OpenTelemetry;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

// <ResourceSetup>
builder.AddOpenTelemetry(o =>
{
    o.ResourceBuilder = ResourceBuilder.CreateDefault()
        .AddService("Orders", serviceVersion: "1.0");
});
// </ResourceSetup>

var app = builder.Build();
app.Run();
