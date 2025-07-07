// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Operations.ServiceDefaults.Messaging.Telemetry;
using System.Diagnostics.Metrics;

namespace Operations.ServiceDefaults.OpenTelemetry;

/// <summary>
///     Provides extension methods for configuring OpenTelemetry instrumentation.
/// </summary>
public static class OpenTelemetrySetupExtensions
{
    /// <summary>
    ///     Adds OpenTelemetry instrumentation for logging, metrics, and distributed tracing.
    /// </summary>
    /// <param name="builder">The host application builder to configure.</param>
    /// <returns>The configured host application builder for method chaining.</returns>
    /// <remarks>
    ///     This method configures:
    ///     <list type="bullet">
    ///         <item>Activity source for custom tracing with configurable name</item>
    ///         <item>Messaging meter for custom metrics collection</item>
    ///         <item>OpenTelemetry logging with formatted messages and scopes</item>
    ///         <item>OTLP exporter for sending telemetry data</item>
    ///         <item>Metrics collection for ASP.NET Core, HTTP client, runtime, and Wolverine</item>
    ///         <item>Distributed tracing with filtering for health check endpoints</item>
    ///         <item>HTTP client instrumentation with path filtering</item>
    ///     </list>
    ///     Configuration can be customized via:
    ///     <list type="bullet">
    ///         <item>OpenTelemetry:ActivitySourceName - Custom activity source name (defaults to application name)</item>
    ///         <item>OpenTelemetry:MessagingMeterName - Custom meter name for messaging (defaults to {AppName}.Messaging)</item>
    ///     </list>
    /// </remarks>
    public static IHostApplicationBuilder AddOpenTelemetry(this IHostApplicationBuilder builder)
    {
        var activitySourceName = builder.Configuration.GetValue<string>("OpenTelemetry:ActivitySourceName")
                                 ?? builder.Environment.ApplicationName;

        var messagingMeterName = builder.Configuration.GetValue<string>("OpenTelemetry:MessagingMeterName")
                                 ?? $"{builder.Environment.ApplicationName}.Messaging";

        var activitySource = new ActivitySource(activitySourceName);
        builder.Services.AddSingleton(activitySource);

        builder.Services.AddKeyedSingleton<Meter>(MessagingMeterStore.MessagingMeterKey, (provider, _) =>
            provider.GetRequiredService<IMeterFactory>().Create(messagingMeterName));

        builder.Services.AddSingleton<MessagingMeterStore>();

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .UseOtlpExporter()
            .WithMetrics(metrics => metrics
                .AddMeter(activitySourceName)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter(nameof(Wolverine))
                .AddMeter(messagingMeterName))
            .WithTracing(tracing => tracing
                .AddSource(activitySourceName)
                .AddAspNetCoreInstrumentation(t => t.Filter = ctx =>
                {
                    if (ctx.Request.Path.Value?.StartsWith("/health/") == true)
                        return false;

                    return true;
                })
                .AddHttpClientInstrumentation(ctx =>
                {
                    ctx.FilterHttpRequestMessage = message =>
                    {
                        var requestPath = message.RequestUri?.AbsolutePath;

                        if (requestPath is null)
                            return true;

                        return !ExcludedClientPaths.Any(requestPath.Contains);
                    };
                }));

        return builder;
    }

    private static readonly List<string> ExcludedClientPaths =
    [
        "/OrleansSiloInstances",
        "/$batch"
    ];
}
