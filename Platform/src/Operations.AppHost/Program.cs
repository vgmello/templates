var builder = DistributedApplication.CreateBuilder(args);

var pgsql = builder.AddPostgres("operations-database");
var database = pgsql.AddDatabase(name: "OperationsDatabase", databaseName: "operations");

builder
    .AddProject<Projects.Accounting_Api>("accounting-api")
    .WithReference(database, connectionName: "AccountingDatabase")
    .WithReference(database, connectionName: "Masstransit");

builder
    .AddProject<Projects.Billing_Api>("billing-api")
    .WithReference(database, connectionName: "BillingDatabase")
    .WithReference(database, connectionName: "Masstransit");

await builder.Build().RunAsync();
