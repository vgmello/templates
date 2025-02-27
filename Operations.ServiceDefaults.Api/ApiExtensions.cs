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

        return builder;
    }

    public static WebApplication ConfigureApiUsingDefaults(this WebApplication app)
    {
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

        app.MapControllers();
        app.MapDefaultEndpoints();

        return app;
    }
}
