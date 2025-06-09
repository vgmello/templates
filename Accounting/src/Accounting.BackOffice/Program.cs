// Copyright (c) ABCDEG. All rights reserved.

using Operations.ServiceDefaults;
using Operations.ServiceDefaults.HealthChecks;

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

app.MapDefaultHealthCheckEndpoints();

await app.RunAsync();
