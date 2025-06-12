var builder = DistributedApplication.CreateBuilder(args);

var pgsql = builder.AddPostgres("operations-database");
var database = pgsql.AddDatabase(name: "OperationsDatabase", databaseName: "operations");

var accountingApiResource = builder // Capture the resource
    .AddProject<Projects.Accounting_Api>("accounting-api")
    .WithReference(database, connectionName: "AccountingDatabase")
    .WithReference(database, connectionName: "Masstransit");

// Add SvelteKit UI
var svelteUI = builder.AddNodeApp("svelte-ui", "../../../svelte_ui", "dev")
                      .WithHttpEndpoint(targetPort: 5173, name: "svelte-ui-http")
                      .WithExternalHttpEndpoints() // To make it accessible from browser
                      .WithEnvironment("NODE_ENV", "development");

// Pass the Accounting.Api endpoint to the SvelteKit app
// Assuming the gRPC endpoint for Accounting.Api is named "http".
// If it's "https", that should be used. Aspire by default uses "http" and "https".
// For gRPC-Web, "http" is typically used unless HTTPS is enforced everywhere.
svelteUI.WithEnvironment("VITE_API_URL", accountingApiResource.GetEndpoint("http").Property(EndpointProperty.Url));


builder
    .AddProject<Projects.Billing_Api>("billing-api")
    .WithReference(database, connectionName: "BillingDatabase")
    .WithReference(database, connectionName: "Masstransit");

await builder.Build().RunAsync();
