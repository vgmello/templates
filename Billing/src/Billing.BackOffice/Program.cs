// Copyright (c) ABCDEG. All rights reserved.

using Operations.ServiceDefaults;
using Operations.ServiceDefaults.HealthChecks;
using Wolverine.Runtime;

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
var app = builder.Build();

app.MapDefaultHealthCheckEndpoints();

await app.RunAsync();
