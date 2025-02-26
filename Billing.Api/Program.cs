// Copyright (c) ABCDEG. All rights reserved.

using Billing;
using MassTransit;
using Operations.ServiceDefaults;
using Operations.ServiceDefaults.Infrastructure.MassTransit;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddServiceDefaults();
builder.AddMassTransit();

builder.Services.AddMediator(cfg =>
{
    cfg.AddConsumersFromNamespaceContaining(typeof(BillingDomain));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapDefaultEndpoints();

await app.RunAsync();


