// Copyright (c) ABCDEG. All rights reserved.

using Operations.ServiceDefaults;
using Operations.ServiceDefaults.Api;
using Operations.ServiceDefaults.HealthChecks;

[assembly: DomainAssembly(typeof(IBillingAssembly))]

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();
builder.AddApiServiceDefaults();

// Application Services
builder.AddApplicationServices();

var app = builder.Build();

app.ConfigureApiUsingDefaults(requireAuth: false);
app.MapDefaultHealthCheckEndpoints();

await app.RunAsync(args);
