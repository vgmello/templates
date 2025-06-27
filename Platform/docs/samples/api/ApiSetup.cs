using Operations.ServiceDefaults;
using Operations.ServiceDefaults.Api;

var builder = WebApplication.CreateBuilder(args);

// #region ApiServiceDefaults
// Add service defaults
builder.AddServiceDefaults();

// Add API-specific defaults
builder.AddApiServiceDefaults();
// #endregion

var app = builder.Build();

// #region ApiConfiguration
// Configure API with defaults
app.ConfigureApiUsingDefaults();

// Map default endpoints
app.MapDefaultEndpoints();
// #endregion

await app.RunAsync(args);