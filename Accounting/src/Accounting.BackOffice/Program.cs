// Copyright (c) ABCDEG. All rights reserved.

var builder = WebApplication.CreateSlimBuilder(args);

var app = builder.Build();

await app.RunAsync();
