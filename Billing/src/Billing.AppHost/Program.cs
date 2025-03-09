// Copyright (c) ABCDEG. All rights reserved.

var builder = DistributedApplication.CreateBuilder(args);

var pgsql = builder.AddPostgres("billing-database");
var database = pgsql.AddDatabase(name: "BillingDatabase", databaseName: "billing");

builder
    .AddProject<Projects.Billing_Api>("billing-api")
    .WithReference(database);

builder
    .AddProject<Projects.Billing_BackOffice>("billing-backoffice")
    .WithReference(database);

await builder.Build().RunAsync();
