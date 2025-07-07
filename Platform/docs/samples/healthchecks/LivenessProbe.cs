using Operations.ServiceDefaults;
using Operations.ServiceDefaults.Api;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApiServiceDefaults();

var app = builder.Build();

// <LivenessEndpoint>
// The /status endpoint is automatically configured by Service Defaults
// It provides a fast, lightweight liveness check that returns:
// - 200 OK with "Healthy" for healthy services
// - 503 Service Unavailable with "Unhealthy" for unhealthy services

// The liveness probe uses cached health check results and executes quickly
// without running expensive health check logic on every request
// </LivenessEndpoint>

app.MapDefaultEndpoints(); // Includes /status endpoint

app.Run();