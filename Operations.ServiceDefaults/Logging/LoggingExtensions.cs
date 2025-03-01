// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Settings.Configuration;

namespace Operations.ServiceDefaults.Logging;

public static class LoggingExtensions
{
    public static IHostApplicationBuilder AddLogging(this IHostApplicationBuilder builder)
    {
        var logLevelSwitch = new LoggingLevelSwitch { MinimumLevel = LevelAlias.Off };
        builder.Services.AddKeyedSingleton(logLevelSwitch, nameof(LoggingExtensions));

        builder.Services
            .AddOptions<DynamicLogLevelSettings>()
            .BindConfiguration(DynamicLogLevelSettings.SectionName);

        builder.Logging.ClearProviders();
        builder.Services.AddSerilog((services, logger) =>
        {
            var dynamicLogLevelSettings = services.GetRequiredService<IOptionsMonitor<DynamicLogLevelSettings>>();

            var standardLogger = ConfigureNewLogger(services, builder.Configuration).CreateLogger();
            var debuggerLogger = ConfigureNewLogger(services, builder.Configuration)
                .MinimumLevel.ControlledBy(logLevelSwitch)
                .Enrich.WithProperty("Diagnostics", "true")
                .Filter.With(new DynamicPropertyLogFilter(standardLogger, dynamicLogLevelSettings, logLevelSwitch))
                .CreateLogger();
            
            logger
                .ReadFrom.Configuration(builder.Configuration)
                .MinimumLevel.Verbose()
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
