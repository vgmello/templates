// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add platform service defaults
builder.AddServiceDefaults();

// Add specific platform features
builder.Services.AddOpenApiDocumentation();
builder.Services.AddWolverineMessaging();

var app = builder.Build();

// Configure platform middleware
app.MapDefaultEndpoints();
app.UseOpenApiDocumentation();

app.Run();
