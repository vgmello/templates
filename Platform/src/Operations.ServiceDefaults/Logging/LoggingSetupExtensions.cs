// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Operations.ServiceDefaults.Logging;

public static class LoggingSetupExtensions
{
    public static IHostApplicationBuilder AddLogging(this IHostApplicationBuilder builder)
    {
        var logLevelSwitch = new LoggingLevelSwitch { MinimumLevel = LevelAlias.Off };
        builder.Services.AddKeyedSingleton(logLevelSwitch, "DynamicLogLevelSwitch");

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

            var config = GetMainLoggerConfig(builder.Configuration);

            logger
                .ReadFrom.Configuration(config)
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

    private static IConfigurationRoot GetMainLoggerConfig(IConfiguration configuration)
    {
        var serilogSection = configuration.GetSection("Serilog");
        var serilogConfigWithoutWriteTo = serilogSection.AsEnumerable()
            .Where(kv => kv.Value is not null && !kv.Key.StartsWith("Serilog:WriteTo"))
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        return new ConfigurationBuilder()
            .AddInMemoryCollection(serilogConfigWithoutWriteTo).Build();
    }
}
