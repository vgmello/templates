// Copyright (c) ABCDEG. All rights reserved.

using Housekeeping.BackOffice.Orleans;
using Housekeeping.BackOffice.Orleans.Infrastructure.Extensions;
using Housekeeping.BackOffice.Orleans.Rooms.Grains;
using Operations.ServiceDefaults;
using Operations.ServiceDefaults.HealthChecks;

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();
builder.AddOrleans();

// Application Services
builder.AddApplicationServices();

var app = builder.Build();

app.MapOrleansDashboard();
app.MapDefaultHealthCheckEndpoints();

app.MapPost("/rooms/{roomNumber}/update-status", async (int roomNumber, string status, IGrainFactory grains) =>
{
    var grain = grains.GetGrain<IRoomGrain>(roomNumber);
    await grain.UpdateStatus(status);

    return Results.Accepted();
});

app.MapGet("/rooms/{roomNumber}", async (int roomNumber, IGrainFactory grains) =>
{
    var grain = grains.GetGrain<IRoomGrain>(roomNumber);

    return Results.Ok(await grain.GetState());
});

await app.RunAsync(args);