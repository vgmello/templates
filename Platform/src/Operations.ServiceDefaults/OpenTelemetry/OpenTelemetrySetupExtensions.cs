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

public static class OpenTelemetrySetupExtensions
{
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
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation());

        return builder;
    }
}
