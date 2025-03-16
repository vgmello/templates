// Copyright (c) ABCDEG. All rights reserved.

using Accounting;
using Operations.ServiceDefaults;
using Operations.ServiceDefaults.Api;
using Wolverine.Runtime;

[assembly: DomainAssembly(typeof(IAccountingAssembly))]

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddApiServiceDefaults();
builder.AddServiceDefaults();

var app = builder.Build();

app.ConfigureApiUsingDefaults();

var x = app.Services.GetService<IWolverineRuntime>();

await app.RunAsync();
