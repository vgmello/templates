// Copyright (c) ABCDEG. All rights reserved.

using Azure.Data.Tables;
using Billing.BackOffice.Orleans;
using Operations.ServiceDefaults;
using Operations.ServiceDefaults.HealthChecks;

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();

// Application Services
builder.AddApplicationServices();

builder.UseOrleans(siloBuilder =>
{
    if (builder.Configuration.GetValue<bool>("Orleans:UseLocalhostClustering"))
    {
        siloBuilder.UseLocalhostClustering();
    }
    // else
    // {
    //     siloBuilder.AddAzureTableGrainStorage()
    // }
    //
    // siloBuilder.AddAzureTableGrainStorageAsDefault(options =>
    //     options.TableServiceClient = new TableServiceClient(context.Configuration.GetConnectionString("OrleansGrainState")));

    siloBuilder.UseDashboard(options =>
    {
        options.HostSelf = false;
        options.Host = "*";
    });
});

var app = builder.Build();

app.MapDefaultHealthCheckEndpoints();

app.Map("/dashboard", x => x.UseOrleansDashboard());

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
