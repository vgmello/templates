// Copyright (c) ABCDEG. All rights reserved.

using Billing.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

// From user secrets Parameters:DbPassword
var dbPassword = builder.AddParameter("DbPassword", secret: true);

var pgsql = builder
    .AddPostgres("billing-db", password: dbPassword)
    .WithContainerName("billing-db")
    .WithPgAdmin(pgAdmin => pgAdmin
        .WithImage("dpage/pgadmin4", "latest")
        .WithLifetime(ContainerLifetime.Persistent))
    .WithLifetime(ContainerLifetime.Persistent);

var database = pgsql.AddDatabase(name: "BillingDb", databaseName: "billing");
var serviceBusDb = pgsql.AddDatabase(name: "ServiceBusDb", databaseName: "service_bus");
var liquibase = builder.AddLiquibaseMigrations(pgsql, dbPassword);

var storage = builder.AddAzureStorage("billing-azure-storage").RunAsEmulator();
var clustering = storage.AddTables("OrleansClustering");
var grainTables = storage.AddTables("OrleansGrainState");

var orleans = builder
    .AddOrleans("billing-orleans")
    .WithClustering(clustering)
    .WithGrainStorage("Default", grainTables);

builder
    .AddProject<Projects.Billing_Api>("billing-api")
    .WithEnvironment("ServiceBus__ConnectionString", serviceBusDb)
    .WithReference(database)
    .WithReference(serviceBusDb)
    .WaitForCompletion(liquibase)
    .WithHttpHealthCheck("/health/internal");

builder
    .AddProject<Projects.Billing_BackOffice>("billing-backoffice")
    .WithEnvironment("ServiceBus__ConnectionString", serviceBusDb)
    .WithReference(database)
    .WithReference(serviceBusDb)
    .WaitForCompletion(liquibase)
    .WithHttpHealthCheck("/health/internal");

builder
    .AddProject<Projects.Billing_BackOffice_Orleans>("billing-backoffice-orleans")
    .WithEnvironment("ServiceBus__ConnectionString", serviceBusDb)
    .WithEnvironment("Orleans__UseLocalhostClustering", "false")
    .WithEnvironment("Aspire__Azure__Data__Tables__DisableHealthChecks", "true")
    .WithReference(orleans)
    .WithReference(database)
    .WithReference(serviceBusDb)
    .WaitForCompletion(liquibase)
    .WithReplicas(3)
    .WithHttpHealthCheck("/health/internal");

await builder.Build().RunAsync();
