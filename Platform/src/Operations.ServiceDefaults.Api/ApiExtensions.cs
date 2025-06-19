// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace Operations.ServiceDefaults.Api;

public static class ApiExtensions
{
    public static IHostApplicationBuilder AddApiServiceDefaults(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddProblemDetails();
        builder.Services.AddOpenApi();
        builder.Services.AddHttpLogging();

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
            app.MapOpenApi();
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
