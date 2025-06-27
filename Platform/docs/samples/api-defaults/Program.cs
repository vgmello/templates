using Operations.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApiServiceDefaults();

var app = builder.Build();

app.ConfigureApiUsingDefaults();

app.Run();
