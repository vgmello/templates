// Copyright (c) ABCDEG. All rights reserved.

using Accounting.Api;
using JasperFx;
using Operations.ServiceDefaults;
using Operations.ServiceDefaults.Api;
using Operations.ServiceDefaults.HealthChecks;
using Accounting.Api.ResourceManagement; // Added for ResourceManagerServiceImpl

[assembly: DomainAssembly(typeof(IAccountingAssembly))]

var builder = WebApplication.CreateSlimBuilder(args);

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSvelteDev",
        policyBuilder => policyBuilder.WithOrigins("http://localhost:5173") // SvelteKit dev server
                              .AllowAnyMethod()
                              .AllowAnyHeader()
                              .AllowCredentials()); // If your gRPC calls need credentials
});

builder.AddServiceDefaults();
builder.AddApiServiceDefaults();

// Application Services
builder.AddApplicationServices();

var app = builder.Build();

app.ConfigureApiUsingDefaults(requireAuth: false);
app.MapGrpcService<ResourceManagerServiceImpl>(); // Register the new gRPC service
app.MapDefaultHealthCheckEndpoints();

return await app.RunJasperFxCommands(args);
