// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.HttpLogging; // Added for HttpLoggingOptions
using Microsoft.OpenApi.Models; // Added for OpenApiInfo

namespace Operations.ServiceDefaults.Api;

public static class ApiExtensions
{
    public static IHostApplicationBuilder AddApiServiceDefaults(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddProblemDetails();
        // Replace AddOpenApi with AddSwaggerGen
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = $"{builder.Environment.ApplicationName} API", Version = "v1" });
        });
        builder.Services.AddHttpLogging(options => { }); // Provide empty lambda for configureOptions

        builder.Services.AddGrpc();
        builder.Services.AddGrpcReflection();

        // Authentication and authorization services
        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();

        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.AddServerHeader = false;
        });

        return builder;
    }

    public static WebApplication ConfigureApiUsingDefaults(this WebApplication app, bool requireAuth = true)
    {
        app.UseHttpLogging();
        app.UseRouting();
        app.UseCors("AllowSvelteDev"); // Apply the CORS policy
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
            app.UseExceptionHandler();
        }

        if (app.Environment.IsDevelopment())
        {
            // Replace MapOpenApi with UseSwagger and UseSwaggerUI
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{app.Environment.ApplicationName} v1");
                // Optionally, configure other SwaggerUI options here
            });
            app.MapScalarApiReference(options => options.WithTitle($"{app.Environment.ApplicationName} OpenAPI"));
            app.MapGrpcReflectionService();
        }

        var controllersEndpointBuilder = app.MapControllers();

        if (requireAuth)
            controllersEndpointBuilder.RequireAuthorization();

        app.MapGrpcServices();

        return app;
    }
}
