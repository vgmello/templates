// Copyright (c) ABCDEG. All rights reserved.

using Operations.ServiceDefaults;
using Operations.ServiceDefaults.HealthChecks;

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();
builder.AddKeyedAzureTableClient("clustering");
builder.AddKeyedAzureTableClient("grain-state");

// Add database connection for health checks
builder.AddNpgsqlDataSource("BillingDb");

// Add health checks for databases and Orleans dependencies
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("BillingDb")!,
        name: "billing-db",
        tags: ["ready"])
    .AddNpgSql(
        builder.Configuration.GetConnectionString("ServiceBusDb") ??
        builder.Configuration.GetConnectionString("ServiceBus__ConnectionString")!,
        name: "servicebus-db",
        tags: ["ready"])
    .AddAzureTable(
        options => options.ConnectionString = builder.Configuration.GetConnectionString("Clustering")!,
        name: "azure-table-clustering",
        tags: ["ready"])
    .AddAzureTable(
        options => options.ConnectionString = builder.Configuration.GetConnectionString("GrainState")!,
        name: "azure-table-grainstate",
        tags: ["ready"])
    .AddOrleansCluster(name: "orleans-cluster", tags: ["ready"]);

builder.Host.UseOrleans((context, siloBuilder) =>
{
    siloBuilder.UseAzureStorageClustering(options =>
        options.ConfigureTableServiceClient(
            context.Configuration.GetConnectionString("Clustering")));

    siloBuilder.AddAzureTableGrainStorageAsDefault(options =>
        options.ConfigureTableServiceClient(
            context.Configuration.GetConnectionString("GrainState")));

    siloBuilder.Configure<Orleans.Configuration.ClusterOptions>(
        context.Configuration.GetSection("Orleans"));
});

var app = builder.Build();

app.MapDefaultHealthCheckEndpoints();

app.MapPost("/invoices/{id:guid}/pay", async (Guid id, decimal amount, IGrainFactory grains) =>
{
    var grain = grains.GetGrain<Billing.BackOffice.Orleans.Grains.IInvoiceGrain>(id);
    await grain.Pay(amount);

    return Results.Accepted();
});

app.MapGet("/invoices/{id:guid}", async (Guid id, IGrainFactory grains) =>
{
    var grain = grains.GetGrain<Billing.BackOffice.Orleans.Grains.IInvoiceGrain>(id);

    return Results.Ok(await grain.GetState());
});

await app.RunAsync();
