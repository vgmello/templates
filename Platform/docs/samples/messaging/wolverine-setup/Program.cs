var builder = WebApplication.CreateBuilder(args);

builder.AddWolverine();

var app = builder.Build();
app.Run();

