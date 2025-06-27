// Service Defaults pattern example
using Operations.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Single call configures all cross-cutting concerns
builder.AddServiceDefaults();

// Add your application-specific services
builder.Services.AddScoped<IMyService, MyService>();

var app = builder.Build();

// Use service defaults
app.MapDefaultEndpoints();

await app.RunAsync();