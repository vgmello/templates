// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using Orleans.Configuration;

namespace Housekeeping.BackOffice.Orleans;

public static class DependencyInjection
{
    public static IServiceCollection AddOrleansServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOrleans(builder =>
        {
            builder
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "housekeeping-cluster";
                    options.ServiceId = "HousekeepingService";
                })
                .ConfigureLogging(logging => logging.AddConsole())
                .UseDashboard(options =>
                {
                    options.Host = "*";
                    options.Port = 8204;
                    options.HostSelf = true;
                });
        });

        return services;
    }

    public static WebApplication UseOrleansApp(this WebApplication app)
    {
        app.MapDefaultEndpoints();
        return app;
    }
}