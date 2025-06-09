// Copyright (c) ABCDEG. All rights reserved.

using Accounting;
using Accounting.Api;
using JasperFx;
using Operations.ServiceDefaults;
using Operations.ServiceDefaults.Api;

[assembly: DomainAssembly(typeof(IAccountingAssembly))]

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();
builder.AddApiServiceDefaults();

builder.AddApplicationServices();

var app = builder.Build();

app.ConfigureApiUsingDefaults(requireAuth: false);

return await app.RunJasperFxCommands(args);

#pragma warning disable S1118 // Utility classes should be static
namespace Accounting.Api
{
    public partial class Program; // Note: Remove this after .NET 10 migration
}
