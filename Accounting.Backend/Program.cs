// Copyright (c) ABCDEG. All rights reserved.

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
}

await app.RunAsync();
