// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Operations.ServiceDefaults.Infrastructure.HealthChecks;
using Operations.ServiceDefaults.Infrastructure.Logging;
using Operations.ServiceDefaults.Infrastructure.Mediator;
using Operations.ServiceDefaults.Infrastructure.OpenTelemetry;

namespace Operations.ServiceDefaults;

public static class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.AddLogging();
        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();
        builder.AddMediator();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

#pragma warning disable S125
        // Uncomment the following to restrict the allowed schemes for service discovery.
        // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
        // {
        //     options.AllowedSchemes = ["https"];
        // });
#pragma warning restore S125

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        app.MapDefaultHealthCheckEndpoints();

        return app;
    }
}
