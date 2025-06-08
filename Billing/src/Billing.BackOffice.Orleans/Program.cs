using Microsoft.Extensions.Hosting;
using Orleans.Hosting;
using Operations.ServiceDefaults;
using Operations.ServiceDefaults.HealthChecks;

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();

builder.Host.UseOrleans((ctx, siloBuilder) =>
{
    var connectionString = ctx.Configuration.GetConnectionString("OrleansStorage") ?? "UseDevelopmentStorage=true";
    siloBuilder
        .UseAzureStorageClustering(options => options.ConfigureTableServiceClient(connectionString))
        .AddAzureTableGrainStorageAsDefault(options => options.ConfigureTableServiceClient(connectionString));
});

var app = builder.Build();

app.MapDefaultHealthCheckEndpoints();

await app.RunAsync();
