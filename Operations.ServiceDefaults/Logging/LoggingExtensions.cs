// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Operations.ServiceDefaults.Logging;

public static class LoggingExtensions
{
    public static IHostApplicationBuilder AddLogging(this IHostApplicationBuilder builder)
    {
        var logLevelSwitch = new LoggingLevelSwitch { MinimumLevel = LevelAlias.Off };
        var monitoredLogProperties = new MonitoredLogProperties();

        builder.Services.AddSingleton(logLevelSwitch);
        builder.Services.AddSingleton(monitoredLogProperties);

        builder.Logging.ClearProviders();
        builder.Services.AddSerilog((services, logger) =>
        {
            var standardLogger = ConfigureNewLogger(services, builder.Configuration).CreateLogger();
            var debuggerLogger = ConfigureNewLogger(services, builder.Configuration)
                .MinimumLevel.ControlledBy(logLevelSwitch)
                .Enrich.WithProperty("Diagnostics", "True")
                .Filter.With(new DynamicPropertyLogFilter(monitoredLogProperties, standardLogger))
                .CreateLogger();

            logger
                .MinimumLevel.Verbose()
                .ReadFrom.Configuration(builder.Configuration)
                .WriteTo.Logger(standardLogger)
                .WriteTo.Logger(debuggerLogger);
        });

        return builder;
    }

    private static LoggerConfiguration ConfigureNewLogger(IServiceProvider services, IConfiguration configuration) =>
        new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(services)
            .WriteTo.OpenTelemetry()
            .Enrich.FromLogContext();
}
