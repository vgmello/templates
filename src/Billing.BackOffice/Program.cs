// Copyright (c) ABCDEG. All rights reserved.

using Billing.BackOffice;
using Billing.Infrastructure;
using Operations.ServiceDefaults;
using Operations.ServiceDefaults.HealthChecks;

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();

// Application Services
builder.AddBillingServices();
builder.AddApplicationServices();

var app = builder.Build();

app.MapDefaultHealthCheckEndpoints();

await app.RunAsync(args);
