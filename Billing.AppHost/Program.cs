var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddPostgres("billing-database");

var api = builder
    .AddProject<Projects.Billing_Api>("billing-api")
    .WithReference(database);

var backend = builder
    .AddProject<Projects.Billing_Backend>("billing-backend")
    .WithReference(database);

await builder.Build().RunAsync();
