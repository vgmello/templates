// Copyright (c) ABCDEG. All rights reserved.

var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL server
var postgres = builder.AddPostgres("postgres", port: 54320)
    .WithDataVolume()
    .WithPgAdmin();

// Databases
var housekeepingDb = postgres.AddDatabase("HousekeepingDb", databaseName: "housekeeping");
var serviceBusDb = postgres.AddDatabase("ServiceBus", databaseName: "service_bus");

// Core API
var api = builder.AddProject<Projects.Housekeeping_Api>("housekeeping-api")
    .WithReference(housekeepingDb)
    .WithReference(serviceBusDb)
    .WaitFor(postgres);

// Background services
var backOffice = builder.AddProject<Projects.Housekeeping_BackOffice>("housekeeping-backoffice")
    .WithReference(housekeepingDb)
    .WithReference(serviceBusDb)
    .WaitFor(postgres);

var orleans = builder.AddProject<Projects.Housekeeping_BackOffice_Orleans>("housekeeping-orleans")
    .WithReference(housekeepingDb)
    .WithReference(serviceBusDb)
    .WaitFor(postgres);

await builder.Build().RunAsync();