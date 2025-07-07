using Operations.ServiceDefaults.Logging;
var builder = WebApplication.CreateBuilder(args);
builder.AddLogging();
var app = builder.Build();
app.Run();
