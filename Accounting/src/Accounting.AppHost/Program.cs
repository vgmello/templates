// Copyright (c) ABCDEG. All rights reserved.

using Accounting.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

// From user secrets Parameters:DbPassword
var dbPassword = builder.AddParameter("DbPassword", secret: true);

var pgsql = builder
    .AddPostgres("accounting-db", password: dbPassword)
    .WithContainerName("accounting-db")
    .WithPgAdmin(pgAdmin => pgAdmin
        .WithImage("dpage/pgadmin4", "latest")
        .WithLifetime(ContainerLifetime.Persistent))
    .WithLifetime(ContainerLifetime.Persistent);

var database = pgsql.AddDatabase(name: "AccountingDb", databaseName: "accounting");
var serviceBusDb = pgsql.AddDatabase(name: "ServiceBusDb", databaseName: "service_bus");
var liquibase = builder.AddLiquibaseMigrations(pgsql, dbPassword);

var storage = builder.AddAzureStorage("storage").RunAsEmulator();
var clustering = storage.AddTables("clustering");
var grainTables = storage.AddTables("grain-state");

var orleans = builder.AddOrleans("default")
    .WithClustering(clustering)
    .WithGrainStorage("Default", grainTables);

builder
    .AddProject<Projects.Accounting_Api>("accounting-api")
    .WithEnvironment("ServiceBus__ConnectionString", serviceBusDb)
    .WithReference(database)
    .WithReference(serviceBusDb)
    .WaitForCompletion(liquibase);

builder
    .AddProject<Projects.Accounting_BackOffice>("accounting-backoffice")
    .WithEnvironment("ServiceBus__ConnectionString", serviceBusDb)
    .WithReference(database)
    .WithReference(serviceBusDb)
    .WaitForCompletion(liquibase);

builder
    .AddProject<Projects.Accounting_BackOffice_Orleans>("accounting-backoffice-orleans")
    .WithEnvironment("ServiceBus__ConnectionString", serviceBusDb)
    .WithEnvironment("Orleans__UseLocalhostClustering", "false")
    .WithReference(orleans)
    .WithReference(database)
    .WithReference(serviceBusDb)
    .WaitForCompletion(liquibase);

await builder.Build().RunAsync();
