using Microsoft.AspNetCore.Builder;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapScalarApiReference();

app.Run();
