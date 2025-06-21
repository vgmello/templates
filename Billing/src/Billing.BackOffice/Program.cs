// Copyright (c) ABCDEG. All rights reserved.

using Billing.BackOffice;
using Operations.ServiceDefaults;
using Operations.ServiceDefaults.HealthChecks;

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();

// Application Services
builder.AddApplicationServices();

var app = builder.Build();

app.MapDefaultHealthCheckEndpoints();

await app.RunAsync(args);
