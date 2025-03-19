// Copyright (c) ABCDEG. All rights reserved.

var builder = DistributedApplication.CreateBuilder(args);

var pgsql = builder.AddPostgres("billing-db");
var database = pgsql.AddDatabase(name: "BillingDb", databaseName: "billing");
var messagingDb = pgsql.AddDatabase(name: "MessagingDb", databaseName: "message_bus");

builder
    .AddProject<Projects.Billing_Api>("billing-api")
    .WithEnvironment("Messaging__ConnectionString", messagingDb)
    .WithReference(messagingDb)
    .WithReference(database);

builder
    .AddProject<Projects.Billing_BackOffice>("billing-backoffice")
    .WithEnvironment("Messaging__ConnectionString", messagingDb)
    .WithReference(messagingDb)
    .WithReference(database);

await builder.Build().RunAsync();
