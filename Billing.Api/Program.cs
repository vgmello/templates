// Copyright (c) ABCDEG. All rights reserved.

using Billing;
using Billing.Api;
using Operations.ServiceDefaults;
using Operations.ServiceDefaults.Api;

[assembly: DomainAssembly(typeof(IBillingAssembly))]

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddApiServiceDefaults();
builder.AddServiceDefaults();

builder.AddApplicationServices();

var app = builder.Build();

app.ConfigureApiUsingDefaults();

await app.RunAsync();
