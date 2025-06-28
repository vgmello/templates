using Operations.ServiceDefaults.OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);

builder.AddOpenTelemetry();

var app = builder.Build();
app.Run();
