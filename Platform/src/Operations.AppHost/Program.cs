// Copyright (c) ABCDEG. All rights reserved.

var builder = DistributedApplication.CreateBuilder(args);

// From user secrets Parameters:DbPassword
var dbPassword = builder.AddParameter("DbPassword", secret: true);

var pgsql = builder
    .AddPostgres("operations-db", password: dbPassword)
    .WithImage("postgres", "17-alpine")
    .WithContainerName("operations-db")
    .WithPgAdmin(pgAdmin => pgAdmin
        .WithImage("dpage/pgadmin4", "latest")
        .WithLifetime(ContainerLifetime.Persistent))
    .WithLifetime(ContainerLifetime.Persistent);

var accountingDb = pgsql.AddDatabase(name: "AccountingDb", databaseName: "accounting");
var billingDb = pgsql.AddDatabase(name: "BillingDb", databaseName: "billing");
var serviceBusDb = pgsql.AddDatabase(name: "ServiceBus", databaseName: "service_bus");

var liquibase = builder
    .AddContainer("operations-liquibase", "liquibase/liquibase")
    .WithBindMount("../../Billing/infra/Billing.Database/Liquibase", "/liquibase/billing")
    .WithBindMount("../../Accounting/infra/Accounting.Database/Liquibase", "/liquibase/accounting")
    .WithEnvironment("LIQUIBASE_COMMAND_USERNAME", "postgres")
    .WithEnvironment("LIQUIBASE_COMMAND_PASSWORD", dbPassword)
    .WaitFor(pgsql)
    .WithReference(pgsql)
    .WithEntrypoint("/bin/sh")
    .WithArgs("-c",
        """
        # Run service_bus migration first (using billing path since both have same service_bus schema)
        liquibase --url=jdbc:postgresql://operations-db:5432/service_bus update --changelog-file=/liquibase/billing/service_bus/changelog.xml && \
        # Run accounting domain migrations
        liquibase --url=jdbc:postgresql://operations-db:5432/accounting update --changelog-file=/liquibase/accounting/accounting/changelog.xml && \
        # Run billing domain migrations
        liquibase --url=jdbc:postgresql://operations-db:5432/billing update --changelog-file=/liquibase/billing/billing/changelog.xml
        """);

// Kafka messaging
var kafka = builder.AddKafka("Messaging").WithKafkaUI();

// TODO: Add Orleans clustering when Orleans projects are properly configured

// Accounting Services - Port range 8120-8139
builder
    .AddProject<Projects.Accounting_Api>("accounting-api")
    .WithEnvironment("ServiceName", "Accounting")
    .WithReference(accountingDb)
    .WithReference(serviceBusDb)
    .WithReference(kafka)
    .WithHttpEndpoint(port: 8121, name: "http")
    .WithHttpsEndpoint(port: 8131, name: "https")
    .WaitForCompletion(liquibase)
    .WithHttpHealthCheck("/health/internal");

builder
    .AddProject<Projects.Accounting_BackOffice>("accounting-backoffice")
    .WithEnvironment("ServiceName", "Accounting")
    .WithReference(accountingDb)
    .WithReference(serviceBusDb)
    .WithReference(kafka)
    .WithHttpEndpoint(port: 8123, name: "http")
    .WithHttpsEndpoint(port: 8133, name: "https")
    .WaitForCompletion(liquibase)
    .WithHttpHealthCheck("/health/internal");

// TODO: Add Accounting Orleans when properly configured

// Billing Services - Port range 8100-8119
builder
    .AddProject<Projects.Billing_Api>("billing-api")
    .WithEnvironment("ServiceName", "Billing")
    .WithReference(billingDb)
    .WithReference(serviceBusDb)
    .WithReference(kafka)
    .WithHttpEndpoint(port: 8101, name: "http")
    .WithHttpsEndpoint(port: 8111, name: "https")
    .WaitForCompletion(liquibase)
    .WithHttpHealthCheck("/health/internal");

builder
    .AddProject<Projects.Billing_BackOffice>("billing-backoffice")
    .WithEnvironment("ServiceName", "Billing")
    .WithReference(billingDb)
    .WithReference(serviceBusDb)
    .WithReference(kafka)
    .WithHttpEndpoint(port: 8103, name: "http")
    .WithHttpsEndpoint(port: 8113, name: "https")
    .WaitForCompletion(liquibase)
    .WithHttpHealthCheck("/health/internal");

// TODO: Add Billing Orleans when properly configured

// Documentation container for Billing (port 8119)
builder
    .AddContainer("billing-docs", "billing-docfx")
    .WithDockerfile("../../Billing/docs")
    .WithBindMount("../../Billing/", "/app")
    .WithHttpEndpoint(port: 8119, targetPort: 8080, name: "http")
    .WithArgs("docs/docfx.json", "--serve", "--hostname=*", "--logLevel=error")
    .WithUrlForEndpoint("http", url =>
    {
        url.DisplayText = "Billing Documentation";
        url.Url = "/";
    })
    .WithHttpHealthCheck("toc.json");

await builder.Build().RunAsync();
