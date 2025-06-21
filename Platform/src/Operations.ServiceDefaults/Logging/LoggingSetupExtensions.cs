// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Operations.ServiceDefaults.Logging;

public static class LoggingSetupExtensions
{
    /// <summary>
    ///     Sets up a bootstrap logger for use during the two-stage initialization of the app host.
    ///     This ensures that any exceptions or log events during host setup are captured and reported.
    ///     See: https://github.com/serilog/serilog-aspnetcore?tab=readme-ov-file#two-stage-initialization
    /// </summary>
    public static void UseInitializationLogger(this IHost _)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.OpenTelemetry()
            .CreateBootstrapLogger();
    }

    public static IHostApplicationBuilder AddLogging(this IHostApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Services.AddSerilog((services, loggerConfig) => ConfigureLogger(loggerConfig, builder.Configuration, services));

        return builder;
    }

    public static LoggerConfiguration ConfigureLogger(
        LoggerConfiguration loggerConfiguration, IConfiguration configuration, IServiceProvider services) =>
        loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(services)
            .WriteTo.OpenTelemetry()
            .Enrich.FromLogContext();
}
