// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.AspNetCore.Mvc;
using Operations.ServiceDefaults;
using Operations.ServiceDefaults.Infrastructure.MassTransit;

[assembly: ApiController]

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddServiceDefaults();
builder.AddMassTransit();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

await app.RunAsync();
