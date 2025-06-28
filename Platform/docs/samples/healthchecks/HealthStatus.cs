using Microsoft.AspNetCore.Authorization;
using Operations.ServiceDefaults;
using Operations.ServiceDefaults.Api;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApiServiceDefaults();

// Add authentication for the health endpoint
builder.Services.AddAuthentication()
    .AddJwtBearer(); // Configure as needed

builder.Services.AddAuthorization();

var app = builder.Build();

// <HealthEndpoint>
// The /health endpoint is automatically configured by Service Defaults
// It provides comprehensive health information for monitoring systems:
// - Requires authentication/authorization for security
// - Returns detailed JSON with individual health check results
// - Includes status, duration, and custom data for each check
// - Used by monitoring dashboards and alerting systems

// Example response format:
// {
//   "status": "Healthy",
//   "totalDuration": "00:00:00.123",
//   "entries": {
//     "database": {
//       "status": "Healthy",
//       "duration": "00:00:00.045",
//       "data": {}
//     },
//     "external_api": {
//       "status": "Degraded",
//       "duration": "00:00:00.078",
//       "data": { "latency": "250ms" }
//     }
//   }
// }
// </HealthEndpoint>

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints(); // Includes authenticated /health endpoint

app.Run();