
using Operations.ServiceDefaults;
using Operations.ServiceDefaults.Api;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApiServiceDefaults();

var app = builder.Build();

app.MapDefaultEndpoints();

app.Run();
