using Operations.ServiceDefaults;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// #region BasicLogging
// Add service defaults which includes Serilog logging
builder.AddServiceDefaults();

// Logging is automatically configured with:
// - Serilog as the primary provider
// - OpenTelemetry integration
// - Configuration from appsettings.json
// - Two-stage initialization for startup error capture
// #endregion

var app = builder.Build();

// #region LoggingUsage
// Use logging throughout your application
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application starting up");

app.MapGet("/example", (ILogger<Program> logger) =>
{
    logger.LogInformation("Processing example request");
    return Results.Ok("Hello, World!");
});
// #endregion

await app.RunAsync();