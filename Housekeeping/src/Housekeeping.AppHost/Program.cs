// Copyright (c) ABCDEG. All rights reserved.

var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL server
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin();

// Databases
var housekeepingDb = postgres.AddDatabase("housekeeping");
var serviceBusDb = postgres.AddDatabase("service-bus");

// Database migrations
var database = builder.AddProject<Projects.Housekeeping_Database>("housekeeping-database")
    .WithReference(housekeepingDb)
    .WithReference(serviceBusDb)
    .WaitFor(postgres);

// Core API
var api = builder.AddProject<Projects.Housekeeping_Api>("housekeeping-api")
    .WithReference(housekeepingDb)
    .WithReference(serviceBusDb)
    .WaitFor(database);

// Background services
var backOffice = builder.AddProject<Projects.Housekeeping_BackOffice>("housekeeping-backoffice")
    .WithReference(housekeepingDb)
    .WithReference(serviceBusDb)
    .WaitFor(database);

var orleans = builder.AddProject<Projects.Housekeeping_BackOffice_Orleans>("housekeeping-orleans")
    .WithReference(housekeepingDb)
    .WithReference(serviceBusDb)
    .WaitFor(database);

builder.Build().Run();