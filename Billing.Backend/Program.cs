// Copyright (c) ABCDEG. All rights reserved.

var builder = WebApplication.CreateSlimBuilder(args);

// Add services to the container.
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

await app.RunAsync();
