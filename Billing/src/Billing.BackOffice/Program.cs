// Copyright (c) ABCDEG. All rights reserved.

using Operations.ServiceDefaults;
using Operations.ServiceDefaults.HealthChecks;

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();

// Add database connection for health checks
builder.AddNpgsqlDataSource("BillingDb");

// Add health checks for databases
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("BillingDb")!,
        name: "billing-db",
        tags: ["ready"])
    .AddNpgSql(
        builder.Configuration.GetConnectionString("ServiceBusDb") ?? 
        builder.Configuration.GetConnectionString("ServiceBus__ConnectionString")!,
        name: "servicebus-db", 
        tags: ["ready"]);

// Add services to the container.
var app = builder.Build();

app.MapDefaultHealthCheckEndpoints();

await app.RunAsync();
