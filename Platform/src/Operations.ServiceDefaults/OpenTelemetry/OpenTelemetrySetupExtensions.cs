// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Operations.ServiceDefaults.OpenTelemetry;

public static class OpenTelemetrySetupExtensions
{
    public static IHostApplicationBuilder AddOpenTelemetry(this IHostApplicationBuilder builder)
    {
        var activitySourceName = builder.Configuration.GetValue<string>("OpenTelemetry:ActivitySourceName")
                                 ?? builder.Environment.ApplicationName;
        var commandMeterName = builder.Configuration.GetValue<string>("OpenTelemetry:CommandMeterName")
                               ?? $"{builder.Environment.ApplicationName}.Commands";

        var activitySource = new ActivitySource(activitySourceName);
        builder.Services.AddSingleton(activitySource);

        // It's good practice to also register the command Meter instance if other services might need to create instruments
        // without directly using IMeterFactory or if a specific configured Meter instance is preferred.
        var commandMeter = new System.Diagnostics.Metrics.Meter(commandMeterName);
        builder.Services.AddSingleton(commandMeter);

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
                .AddMeter(commandMeterName)) // Add the new meter for command-specific metrics
            .WithTracing(tracing => tracing
                .AddSource(activitySourceName)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource(nameof(Wolverine)));

        return builder;
    }
}
