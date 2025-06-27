using Operations.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add all service defaults with one call
builder.AddServiceDefaults();

var app = builder.Build();

// Configure default endpoints
app.MapDefaultEndpoints();

// Use enhanced startup with proper error handling
await app.RunAsync(args);