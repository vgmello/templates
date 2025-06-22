var builder = DistributedApplication.CreateBuilder(args);

var pgsql = builder.AddPostgres("operations-database");
var serviceBusDb = pgsql.AddDatabase(name: "ServiceBus", databaseName: "service_bus");
var kafka = builder.AddKafka("Messaging");

builder
    .AddProject<Projects.Accounting_Api>("accounting-api")
    .WithEnvironment("ServiceName", "Accounting")
    .WithReference(serviceBusDb)
    .WithReference(kafka);

builder
    .AddProject<Projects.Accounting_BackOffice>("accounting-backoffice")
    .WithEnvironment("ServiceName", "Accounting")
    .WithReference(serviceBusDb)
    .WithReference(kafka);

builder
    .AddProject<Projects.Billing_Api>("billing-api")
    .WithEnvironment("ServiceName", "Billing")
    .WithReference(serviceBusDb)
    .WithReference(kafka);

builder
    .AddProject<Projects.Billing_BackOffice>("billing-backoffice")
    .WithEnvironment("ServiceName", "Billing")
    .WithReference(serviceBusDb)
    .WithReference(kafka);

await builder.Build().RunAsync();
