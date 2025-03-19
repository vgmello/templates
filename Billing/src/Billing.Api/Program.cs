// Copyright (c) ABCDEG. All rights reserved.

using Billing;
using Billing.Api;
using Oakton;
using Operations.ServiceDefaults;
using Operations.ServiceDefaults.Api;

[assembly: DomainAssembly(typeof(IBillingAssembly))]

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();
builder.AddApiServiceDefaults();

builder.AddApplicationServices();

var app = builder.Build();

app.ConfigureApiUsingDefaults(requireAuth: false);

return await app.RunOaktonCommands(args);
