var builder = DistributedApplication.CreateBuilder(args);

var pgsql = builder.AddPostgres("operations-database");
var database = pgsql.AddDatabase(name: "OperationsDatabase", databaseName: "operations");

// Add API services
var accountingApiResource = builder
    .AddProject<Projects.Accounting_Api>("accounting-api")
    .WithReference(database, connectionName: "AccountingDatabase")
    .WithReference(database, connectionName: "Masstransit");

var billingApiResource = builder
    .AddProject<Projects.Billing_Api>("billing-api")
    .WithReference(database, connectionName: "BillingDatabase")
    .WithReference(database, connectionName: "Masstransit");

// Add SvelteKit UI with proper configuration
var svelteUI = builder.AddNodeApp("svelte-ui", "../../../svelte_ui", "dev")
    .WithHttpEndpoint(targetPort: 5173, name: "svelte-ui-http")
    .WithExternalHttpEndpoints() // Make accessible from browser
    .WithEnvironment("NODE_ENV", "development")
    .WithEnvironment("VITE_NODE_ENV", "development");

// Configure API endpoints for SvelteKit
svelteUI
    .WithEnvironment("VITE_ACCOUNTING_API_URL", accountingApiResource.GetEndpoint("http").Property(EndpointProperty.Url))
    .WithEnvironment("VITE_BILLING_API_URL", billingApiResource.GetEndpoint("http").Property(EndpointProperty.Url))
    .WithEnvironment("VITE_PLATFORM_API_URL", "http://localhost:5270"); // Platform API if it exists

// Add dependency relationships
svelteUI.WaitFor(accountingApiResource);
svelteUI.WaitFor(billingApiResource);

await builder.Build().RunAsync();
