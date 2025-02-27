// Copyright (c) ABCDEG. All rights reserved.

using Accounting;
using Operations.ServiceDefaults;
using Operations.ServiceDefaults.Api;

[assembly: DomainAssembly(typeof(IAccountingAssembly))]

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddApiServiceDefaults();
builder.AddServiceDefaults();

var app = builder.Build();

app.ConfigureApiUsingDefaults();

await app.RunAsync();
