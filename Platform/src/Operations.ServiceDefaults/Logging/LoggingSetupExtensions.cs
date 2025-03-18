// Copyright (c) ABCDEG.All rights reserved.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using static Serilog.Core.Constants;

namespace Operations.ServiceDefaults.Logging;

public static class LoggingSetupExtensions
{
    public static IHostApplicationBuilder AddLogging(this IHostApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();

        // TODO: Benchmark this using k6
        // builder.Services.AddSerilog((services, logger) => ConfigureLogger(logger, builder.Configuration, services));
        builder.AddDynamicLogging();

        return builder;
    }

    public static IServiceCollection AddDynamicLogging(this IHostApplicationBuilder builder)
    {
        var logLevelsConfig = GetLogLevelsConfig(builder.Configuration);
        var defaultLogLevel = logLevelsConfig.GetValue<LogEventLevel>("Serilog:MinimumLevel:Default");

        var mainLevelSwitch = new LoggingLevelSwitch(defaultLogLevel);
        var dynamicLogLevelSwitch = new LoggingLevelSwitch { MinimumLevel = LevelAlias.Off };

        dynamicLogLevelSwitch.MinimumLevelChanged += (_, e) =>
            mainLevelSwitch.MinimumLevel = e.NewLevel == LogEventLevel.Debug ? LogEventLevel.Debug : defaultLogLevel;

        builder.Services.AddKeyedSingleton(dynamicLogLevelSwitch, "DynamicLogLevelSwitch");

        builder.Services
            .AddOptions<DynamicLogLevelSettings>()
            .BindConfiguration(DynamicLogLevelSettings.SectionName);

        return builder.Services.AddSerilog((services, logger) =>
        {
            var dynamicLogLevelSettings = services.GetRequiredService<IOptionsMonitor<DynamicLogLevelSettings>>();

            var standardLogger = ConfigureLogger(new LoggerConfiguration(), builder.Configuration, services)
                .CreateLogger();

            var debuggerLogger = ConfigureLogger(new LoggerConfiguration(), builder.Configuration, services)
                .MinimumLevel.ControlledBy(dynamicLogLevelSwitch)
                .Enrich.WithProperty("Diagnostics", "true")
                .Filter.With(new DynamicPropertyLogFilter(standardLogger, dynamicLogLevelSettings,
                    dynamicLogLevelSwitch))
                .CreateLogger();

            logger
                .ReadFrom.Configuration(logLevelsConfig)
                .MinimumLevel.ControlledBy(mainLevelSwitch)
                .WriteTo.Sink(new LogForwardingSink(standardLogger))
                .WriteTo.Sink(debuggerLogger);
        });
    }

    private static LoggerConfiguration ConfigureLogger(
        LoggerConfiguration loggerConfiguration, IConfiguration configuration, IServiceProvider services) =>
        loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(services)
            .WriteTo.OpenTelemetry()
            .Enrich.FromLogContext();

    private static IConfiguration GetLogLevelsConfig(IConfiguration configuration)
    {
        var serilogLogLevelsSection = configuration.GetSection("Serilog:MinimumLevel");
        var serilogLogLevelsConfig = serilogLogLevelsSection.AsEnumerable()
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        return new ConfigurationBuilder()
            .AddInMemoryCollection(serilogLogLevelsConfig).Build();
    }
}

public class LogForwardingSink(Logger logger) : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue(SourceContextPropertyName, out var sourceContext))
            logger.ForContext(SourceContextPropertyName, sourceContext).Write(logEvent);
    }
}
