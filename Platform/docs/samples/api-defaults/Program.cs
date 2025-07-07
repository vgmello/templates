using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Operations.ServiceDefaults.Api;

var builder = WebApplication.CreateBuilder(args);

// Add API service defaults
builder.AddApiServiceDefaults();

var app = builder.Build();

// Configure API using defaults
app.ConfigureApiUsingDefaults();

app.Run();