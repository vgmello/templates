using Operations.ServiceDefaults;
using Operations.ServiceDefaults.Api;

var builder = WebApplication.CreateBuilder(args);

// <BasicHealthChecks>
// Health checks are automatically configured when you add Service Defaults
builder.AddServiceDefaults();
builder.AddApiServiceDefaults();

// Additional services
builder.Services.AddControllers();
// </BasicHealthChecks>

var app = builder.Build();

// Configure the HTTP request pipeline
app.MapDefaultEndpoints();
app.MapControllers();

app.Run();