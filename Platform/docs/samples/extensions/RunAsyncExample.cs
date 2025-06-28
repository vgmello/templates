using Operations.ServiceDefaults;
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
await app.RunAsync(args);
