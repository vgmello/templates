// Copyright (c) ABCDEG. All rights reserved.

using Housekeeping.Api;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDataSource("housekeeping");

builder.Services.AddHousekeepingWolverine(builder.Configuration, builder.GetConnectionString("service-bus"));
builder.Services.AddHousekeepingApi(builder.Configuration);

var app = builder.Build();

app.UseHousekeepingApi();

await app.RunAsync();