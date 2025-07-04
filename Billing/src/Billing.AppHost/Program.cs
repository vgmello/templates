// Copyright (c) ABCDEG. All rights reserved.

using Billing.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

// From user secrets Parameters:DbPassword
var dbPassword = builder.AddParameter("DbPassword", secret: true);

var pgsql = builder
    .AddPostgres("billing-db", password: dbPassword)
    .WithImage("postgres", "17-alpine")
    .WithContainerName("billing-db")
    .WithPgAdmin(pgAdmin => pgAdmin
        .WithImage("dpage/pgadmin4", "latest")
        .WithLifetime(ContainerLifetime.Persistent)
        .WithUrlForEndpoint("http", url => url.DisplayText = "PgAdmin (DB Management)"))
    .WithLifetime(ContainerLifetime.Persistent);

var database = pgsql.AddDatabase(name: "BillingDb", databaseName: "billing");
var serviceBusDb = pgsql.AddDatabase(name: "ServiceBus", databaseName: "service_bus");
var liquibase = builder.AddLiquibaseMigrations(pgsql, dbPassword);

var kafka = builder
    .AddKafka("messaging")
    .WithKafkaUI(r => r.WithUrlForEndpoint("http", url => url.DisplayText = "Kafka UI"));

var storage = builder.AddAzureStorage("billing-azure-storage").RunAsEmulator();
var clustering = storage.AddTables("OrleansClustering");
var grainTables = storage.AddTables("OrleansGrainState");

var orleans = builder
    .AddOrleans("billing-orleans")
    .WithClustering(clustering)
    .WithGrainStorage("Default", grainTables);

var billingApi = builder
    .AddProject<Projects.Billing_Api>("billing-api")
    .WithEnvironment("ServiceName", "Billing")
    .WithKestrelLaunchProfileEndpoints()
    .WithReference(database)
    .WithReference(serviceBusDb)
    .WithReference(kafka)
    .WaitForCompletion(liquibase)
    .WithHttpHealthCheck("/health/internal");

builder
    .AddNpmApp("billing-ui", "../../../Billing/web/billing-ui", "dev")
    .WithEnvironment("GRPC_HOST", () => billingApi.GetGrpcEndpoint().Host)
    .WithEnvironment("GRPC_PORT", () => billingApi.GetGrpcEndpoint().Port.ToString())
    .WithHttpEndpoint(env: "PORT", port: 8105)
    .WithExternalHttpEndpoints()
    .WithReference(billingApi)
    .WaitFor(billingApi)
    .PublishAsDockerFile();

builder
    .AddProject<Projects.Billing_BackOffice>("billing-backoffice")
    .WithEnvironment("ServiceName", "Billing")
    .WithReference(database)
    .WithReference(serviceBusDb)
    .WithReference(kafka)
    .WaitForCompletion(liquibase)
    .WithHttpHealthCheck("/health/internal");

builder
    .AddProject<Projects.Billing_BackOffice_Orleans>("billing-backoffice-orleans")
    .WithEnvironment("ServiceName", "Billing")
    .WithEnvironment("Orleans__UseLocalhostClustering", "false")
    .WithEnvironment("Aspire__Azure__Data__Tables__DisableHealthChecks", "true")
    .WithReference(orleans)
    .WithReference(database)
    .WithReference(serviceBusDb)
    .WithReference(kafka)
    .WaitForCompletion(liquibase)
    .WithReplicas(3)
    .WithUrlForEndpoint("https", url =>
    {
        url.DisplayText = "Dashboard";
        url.Url = "/dashboard";
    })
    .WithHttpHealthCheck("/health/internal");

builder
    .AddContainer("billing-docs", "billing-docfx")
    .WithDockerfile("../../docs")
    .WithBindMount("../../", "/app")
    .WithHttpEndpoint(port: 8119, targetPort: 8080, name: "http")
    .WithArgs("docs/docfx.json", "--serve", "--hostname=*", "--logLevel=error")
    .WithUrlForEndpoint("http", url => url.DisplayText = "App Documentation")
    .WithHttpHealthCheck("toc.json");

await builder.Build().RunAsync();
