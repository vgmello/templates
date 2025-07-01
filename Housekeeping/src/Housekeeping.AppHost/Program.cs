// Copyright (c) ABCDEG. All rights reserved.

using Housekeeping.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

// From user secrets Parameters:DbPassword
var dbPassword = builder.AddParameter("DbPassword", secret: true);

var pgsql = builder
    .AddPostgres("housekeeping-db", password: dbPassword, port: 54320)
    .WithImage("postgres", "17-alpine")
    .WithContainerName("housekeeping-db")
    .WithEndpointProxySupport(false)
    .WithPgAdmin(pgAdmin => pgAdmin
        .WithHostPort(port: 54321)
        .WithEndpointProxySupport(false)
        .WithImage("dpage/pgadmin4", "latest")
        .WithLifetime(ContainerLifetime.Persistent)
        .WithUrlForEndpoint("http", url => url.DisplayText = "PgAdmin (DB Management)"))
    .WithLifetime(ContainerLifetime.Persistent);

// Databases
var database = pgsql.AddDatabase(name: "HousekeepingDb", databaseName: "housekeeping");
var serviceBusDb = pgsql.AddDatabase(name: "ServiceBus", databaseName: "service_bus");
builder.AddLiquibaseMigrations(pgsql, dbPassword);

builder.AddProject<Projects.Housekeeping_Api>("housekeeping-api")
    .WithEnvironment("ServiceName", "Housekeeping")
    .WithKestrelLaunchProfileEndpoints()
    .WithReference(database)
    .WithReference(serviceBusDb)
    .WaitFor(pgsql)
    .WithHttpHealthCheck("/health/internal");

await builder.Build().RunAsync();
