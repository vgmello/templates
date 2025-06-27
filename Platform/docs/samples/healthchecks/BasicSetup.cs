using Operations.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// #region BasicHealthChecks
// Add service defaults which includes health checks
builder.AddServiceDefaults();

// Add custom health checks
builder.Services.AddHealthChecks()
    .AddCheck("sample_check", () => HealthCheckResult.Healthy("Sample is working"));
// #endregion

var app = builder.Build();

// #region HealthCheckEndpoints
// Map default health check endpoints
app.MapDefaultHealthCheckEndpoints();
// #endregion

await app.RunAsync();