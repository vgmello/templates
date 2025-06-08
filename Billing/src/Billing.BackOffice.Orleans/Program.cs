using Microsoft.Extensions.Hosting;
using Operations.ServiceDefaults;
using Operations.ServiceDefaults.HealthChecks;

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();
builder.AddKeyedAzureTableClient("clustering");
builder.AddKeyedAzureTableClient("grain-state");
builder.UseOrleans();

var app = builder.Build();

app.MapDefaultHealthCheckEndpoints();

await app.RunAsync();
