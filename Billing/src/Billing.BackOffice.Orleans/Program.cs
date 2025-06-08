// Copyright (c) ABCDEG. All rights reserved.

using Operations.ServiceDefaults;
using Operations.ServiceDefaults.HealthChecks;

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();
builder.AddKeyedAzureTableClient("clustering");
builder.AddKeyedAzureTableClient("grain-state");
builder.Host.UseOrleans((context, siloBuilder) =>
{
    siloBuilder.UseAzureStorageClustering(options =>
        options.ConfigureTableServiceClient(
            context.Configuration.GetConnectionString("Clustering")));

    siloBuilder.AddAzureTableGrainStorageAsDefault(options =>
        options.ConfigureTableServiceClient(
            context.Configuration.GetConnectionString("GrainState")));

    siloBuilder.Configure<Orleans.Configuration.ClusterOptions>(
        context.Configuration.GetSection("Orleans"));
});

var app = builder.Build();

app.MapDefaultHealthCheckEndpoints();

app.MapPost("/invoices/{id:guid}/pay", async (Guid id, decimal amount, IGrainFactory grains) =>
{
    var grain = grains.GetGrain<Billing.BackOffice.Orleans.Grains.IInvoiceGrain>(id);
    await grain.Pay(amount);

    return Results.Accepted();
});

app.MapGet("/invoices/{id:guid}", async (Guid id, IGrainFactory grains) =>
{
    var grain = grains.GetGrain<Billing.BackOffice.Orleans.Grains.IInvoiceGrain>(id);

    return Results.Ok(await grain.GetState());
});

await app.RunAsync();
