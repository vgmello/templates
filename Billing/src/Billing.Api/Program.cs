// Copyright (c) ABCDEG. All rights reserved.

using JasperFx;
using Operations.ServiceDefaults;
using Wolverine;
// using Wolverine.Transports.Kafka; // Removed as Kafka setup is centralized
using Operations.ServiceDefaults.Api;
using Operations.ServiceDefaults.HealthChecks;
using Operations.ServiceDefaults.Messaging.Wolverine; // For AddWolverine extension and CloudEventWrappingPolicy (if used directly)

[assembly: DomainAssembly(typeof(IBillingAssembly))]

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();
builder.AddApiServiceDefaults();

// Application Services
builder.AddApplicationServices();

// Add Wolverine with centralized defaults from Operations.ServiceDefaults
builder.AddWolverine();
// If service-specific Wolverine opts were needed:
// builder.AddWolverine(opts => { /* service-specific options here */ });

var app = builder.Build();

app.ConfigureApiUsingDefaults(requireAuth: false);
app.MapDefaultHealthCheckEndpoints();

await app.RunAsync();
