// Copyright (c) ABCDEG. All rights reserved.

using Housekeeping.BackOffice.Orleans;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDataSource("housekeeping");

builder.Services.AddHousekeepingWolverine(builder.Configuration, builder.GetConnectionString("service-bus"));
builder.Services.AddOrleansServices(builder.Configuration);

var app = builder.Build();

app.UseOrleansApp();

await app.RunAsync();