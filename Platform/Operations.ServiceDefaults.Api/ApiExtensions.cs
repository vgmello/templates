// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Operations.ServiceDefaults.Api;

public static class ApiExtensions
{
    public static IHostApplicationBuilder AddApiServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddProblemDetails();
        builder.Services.AddSwaggerGen();

        // Authentication and authorization services
        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();

        return builder;
    }

    public static WebApplication ConfigureApiUsingDefaults(this WebApplication app, bool requireAuth = true)
    {
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
            app.UseExceptionHandler();
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        var controllersEndpointBuilder = app.MapControllers();

        if (requireAuth)
            controllersEndpointBuilder.RequireAuthorization();

        app.MapDefaultEndpoints();

        return app;
    }
}
