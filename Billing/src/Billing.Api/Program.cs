// Copyright (c) ABCDEG. All rights reserved.

using Billing;
using Billing.Api;
using JasperFx;
using Operations.ServiceDefaults;
using Operations.ServiceDefaults.Api;
using Operations.ServiceDefaults.HealthChecks;

[assembly: DomainAssembly(typeof(IBillingAssembly))]

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();
builder.AddApiServiceDefaults();

builder.AddApplicationServices();

var app = builder.Build();

app.ConfigureApiUsingDefaults(requireAuth: false);
app.MapDefaultHealthCheckEndpoints();

return await app.RunJasperFxCommands(args);

#pragma warning disable S1118 // Utility classes should be static
public partial class Program; // Note: Remove this after .NET 10 migration
