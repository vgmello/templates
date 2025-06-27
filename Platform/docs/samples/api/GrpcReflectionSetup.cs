using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc().AddGrpcReflection();

var app = builder.Build();

app.MapGrpcReflectionService();

app.Run();
