using Operations.ServiceDefaults;
using Operations.ServiceDefaults.Api;

// <ServiceDefaultsPattern>
// The Service Defaults pattern provides a central place to configure
// cross-cutting concerns that should be consistent across all services

var builder = WebApplication.CreateBuilder(args);

// Single call configures multiple concerns:
// - Structured logging with Serilog
// - Health checks with multiple endpoints (/status, /health/internal, /health)
// - OpenTelemetry tracing and metrics
// - Default HTTP client configurations with resilience
// - Service discovery integration
// - Configuration management
builder.AddServiceDefaults();

// For API services, add additional defaults:
// - OpenAPI documentation support
// - Problem details configuration
// - Endpoint filters for common scenarios
// - API versioning support
builder.AddApiServiceDefaults();

// Application-specific services
builder.Services.AddControllers();
// </ServiceDefaultsPattern>

var app = builder.Build();

// <DefaultEndpoints>
// Service Defaults automatically provides essential endpoints:
app.MapDefaultEndpoints(); // Includes:
                          // - GET /status (liveness probe)
                          // - GET /health/internal (readiness probe, localhost only)
                          // - GET /health (detailed health, authenticated)
                          // - GET /metrics (Prometheus metrics, if configured)
// </DefaultEndpoints>

app.MapControllers();

app.Run();