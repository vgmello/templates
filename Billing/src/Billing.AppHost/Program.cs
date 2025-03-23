// Copyright (c) ABCDEG. All rights reserved.

var builder = DistributedApplication.CreateBuilder(args);

var pgsql = builder.AddPostgres("billing-db");
var database = pgsql.AddDatabase(name: "BillingDb", databaseName: "billing");
var serviceBusDb = pgsql.AddDatabase(name: "ServiceBusDb", databaseName: "service_bus");

builder
    .AddProject<Projects.Billing_Api>("billing-api")
    .WithEnvironment("ServiceBus__ConnectionString", serviceBusDb)
    .WithReference(serviceBusDb)
    .WithReference(database);

builder
    .AddProject<Projects.Billing_BackOffice>("billing-backoffice")
    .WithEnvironment("ServiceBus__ConnectionString", serviceBusDb)
    .WithReference(serviceBusDb)
    .WithReference(database);

await builder.Build().RunAsync();
