// Copyright (c) ABCDEG. All rights reserved.

using Operations.ServiceDefaults;

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
var app = builder.Build();

app.MapDefaultEndpoints();

await app.RunAsync();
