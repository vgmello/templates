var builder = WebApplication.CreateBuilder(args);

// Automatically includes endpoint filters
builder.AddServiceDefaults();

var app = builder.Build();

// Development-only endpoint with localhost filter
app.MapGet("/debug/config", () => builder.Configuration.AsEnumerable())
    .AddEndpointFilter<LocalhostEndpointFilter>();
