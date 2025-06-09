// Copyright (c) ABCDEG. All rights reserved.

using Operations.ServiceDefaults;
using Operations.ServiceDefaults.HealthChecks;

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();
builder.AddKeyedAzureTableClient("clustering");
builder.AddKeyedAzureTableClient("grain-state");

builder.UseOrleans(siloBuilder =>
{
    if (builder.Configuration.GetValue<bool>("Orleans:UseLocalhostClustering"))
    {
        siloBuilder.UseLocalhostClustering();
    }
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
