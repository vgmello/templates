using Operations.ServiceDefaults;
using Operations.ServiceDefaults.Api;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults
builder.AddServiceDefaults();

// Add API-specific defaults
builder.AddApiServiceDefaults();

var app = builder.Build();

// Configure API with defaults
app.ConfigureApiUsingDefaults();

// Map default endpoints
app.MapDefaultEndpoints();

await app.RunAsync(args);