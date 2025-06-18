// Copyright (c) ABCDEG. All rights reserved.

using Accounting.BackOffice.Orleans.Grains;
using Accounting.BackOffice.Orleans.Infrastructure.Extensions;
using Operations.ServiceDefaults;
using Operations.ServiceDefaults.HealthChecks;

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();
builder.AddOrleans();

var app = builder.Build();

app.MapOrleansDashboard();

app.MapDefaultHealthCheckEndpoints();

app.MapPost("/ledgers/{id:guid}/balance", async (Guid id, decimal amount, IGrainFactory grains) =>
{
    var grain = grains.GetGrain<ILedgerGrain>(id);
    await grain.Pay(amount);

    return Results.Accepted();
});

app.MapGet("/ledgers/{id:guid}", async (Guid id, IGrainFactory grains) =>
{
    var grain = grains.GetGrain<ILedgerGrain>(id);

    return Results.Ok(await grain.GetState());
});

await app.RunAsync();
