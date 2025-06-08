// Copyright (c) ABCDEG. All rights reserved.

using Azure.Data.Tables;
using Microsoft.Extensions.Hosting;
using Operations.ServiceDefaults;
using Operations.ServiceDefaults.HealthChecks;
using OrleansDashboard;

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();
builder.AddKeyedAzureTableClient("clustering");
builder.AddKeyedAzureTableClient("grain-state");
builder.Host.UseOrleans((context, siloBuilder) =>
{
    if (context.Configuration.GetValue<bool>("Orleans:UseLocalhostClustering"))
    {
        siloBuilder.UseLocalhostClustering();
    }
    else
    {
        siloBuilder.UseAzureStorageClustering(options =>
            options.TableServiceClient = new TableServiceClient(context.Configuration.GetConnectionString("OrleansClustering")));
    }

    siloBuilder.AddAzureTableGrainStorageAsDefault(options =>
        options.TableServiceClient = new TableServiceClient(context.Configuration.GetConnectionString("OrleansGrainState")));

    siloBuilder.Configure<Orleans.Configuration.ClusterOptions>(context.Configuration.GetSection("Orleans"));
});

var app = builder.Build();

app.Map("/dashboard", x => x.UseOrleansDashboard());

app.MapDefaultHealthCheckEndpoints();

app.MapPost("/invoices/{id:guid}/pay", async (Guid id, decimal amount, IGrainFactory grains) =>
{
    var grain = grains.GetGrain<Accounting.BackOffice.Orleans.Grains.IInvoiceGrain>(id);
    await grain.Pay(amount);

    return Results.Accepted();
});

app.MapGet("/invoices/{id:guid}", async (Guid id, IGrainFactory grains) =>
{
    var grain = grains.GetGrain<Accounting.BackOffice.Orleans.Grains.IInvoiceGrain>(id);

    return Results.Ok(await grain.GetState());
});

await app.RunAsync();
