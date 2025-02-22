var builder = DistributedApplication.CreateBuilder(args);

var pgsql = builder.AddPostgres("billing-database");
var database = pgsql.AddDatabase(name: "BillingDatabase", databaseName: "billing");

builder
    .AddProject<Projects.Billing_Api>("billing-api")
    .WithReference(database);

builder
    .AddProject<Projects.Billing_Backend>("billing-backend")
    .WithReference(database);

await builder.Build().RunAsync();
