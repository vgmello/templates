using Operations.ServiceDefaults;
using Operations.ServiceDefaults.Api;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApiServiceDefaults();

var app = builder.Build();

// <ReadinessEndpoint>
// The /health/internal endpoint is automatically configured by Service Defaults
// It's designed for container orchestration readiness checks:
// - Only accessible from localhost for security
// - Executes health checks to verify application readiness
// - Returns simplified JSON in production, detailed JSON in development
// - Used by Kubernetes readiness probes and Docker health checks
// </ReadinessEndpoint>

app.MapDefaultEndpoints(); // Includes /health/internal endpoint

app.Run();