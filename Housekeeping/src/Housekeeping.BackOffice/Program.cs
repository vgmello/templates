// Copyright (c) ABCDEG. All rights reserved.

using Housekeeping.BackOffice;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDataSource("housekeeping");

builder.Services.AddHousekeepingWolverine(builder.Configuration, builder.GetConnectionString("service-bus"));
builder.Services.AddBackOfficeServices();

var host = builder.Build();
await host.RunAsync();