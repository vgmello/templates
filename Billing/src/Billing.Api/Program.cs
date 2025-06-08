// Copyright (c) ABCDEG. All rights reserved.

using Billing;
using Billing.Api;
using JasperFx;
using Operations.ServiceDefaults;
using Operations.ServiceDefaults.Api;

[assembly: DomainAssembly(typeof(IBillingAssembly))]

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();
builder.AddApiServiceDefaults();

builder.AddApplicationServices();

var app = builder.Build();

app.ConfigureApiUsingDefaults(requireAuth: false);

// Map gRPC services from this API assembly explicitly so that they are
// registered when the API is hosted by the test infrastructure where the
// entry assembly differs from the application assembly.
app.MapGrpcServices(typeof(Program));

return await app.RunJasperFxCommands(args);

#pragma warning disable S1118 // Utility classes should be static
public partial class Program; // Note: Remove this after .NET 10 migration
