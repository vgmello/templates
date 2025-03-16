// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Operations.ServiceDefaults.Logging;
using Operations.ServiceDefaults.OpenTelemetry;
using Operations.ServiceDefaults.Wolverine;
using System.Reflection;

namespace Operations.ServiceDefaults;

public static class Extensions
{
    private static Assembly? _entryAssembly;

    public static Assembly EntryAssembly
    {
        get => _entryAssembly ??= GetEntryAssembly();
        set => _entryAssembly = value;
    }

    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.AddLogging();
        builder.AddOpenTelemetry();
        builder.AddWolverine();

        builder.Services.AddHealthChecks();
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

    private static Assembly GetEntryAssembly()
    {
        return Assembly.GetEntryAssembly() ??
               throw new InvalidOperationException(
                   "Unable to identify entry assembly. Please provide an assembly via the Extensions.AssemblyMarker property.");
    }
}
