// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Operations.ServiceDefaults.Logging;

public static class LoggingSetupExtensions
{
    public static IHostApplicationBuilder AddLogging(this IHostApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Services.AddSerilog((services, logger) => ConfigureLogger(logger, builder.Configuration, services));

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
