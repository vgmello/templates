// Copyright (c) ABCDEG. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Operations.ServiceDefaults.Logging;

public sealed class DynamicPropertyLogFilter : ILogEventFilter, IDisposable
{
    private readonly ILogger _logger;
    private readonly Logger _standardLogger;
    private readonly IDisposable? _propertiesChangedHandler;

    private Dictionary<string, HashSet<string>> _monitoredProperties;

    public DynamicPropertyLogFilter(
        Logger standardLogger, IOptionsMonitor<DynamicLogLevelSettings> dynamicLogLevelSettings,
        LoggingLevelSwitch logLevelSwitch)
    {
        _logger = standardLogger.ForContext<DynamicPropertyLogFilter>();
        _standardLogger = standardLogger;
        _propertiesChangedHandler = dynamicLogLevelSettings.OnChange(s => SettingsChangedHandler(s, logLevelSwitch));

        SettingsChangedHandler(dynamicLogLevelSettings.CurrentValue, logLevelSwitch);
    }

    public bool IsEnabled(LogEvent logEvent)
    {
        if (_monitoredProperties.Count == 0 || _standardLogger.IsEnabled(logEvent.Level))
            return false;

        foreach (var property in logEvent.Properties)
        {
            if (_monitoredProperties.TryGetValue(property.Key, out var values) &&
                values.Contains(property.Value.ToString()))
                return true;
        }

        return false;
    }

    [MemberNotNull(nameof(_monitoredProperties))]
    private void SettingsChangedHandler(DynamicLogLevelSettings settings, LoggingLevelSwitch logLevelSwitch)
    {
        _logger.Information("Updating dynamic log level settings");

        var newLogLevel = settings.Properties is { Count: > 0 } ? LogEventLevel.Debug : LevelAlias.Off;

        if (logLevelSwitch.MinimumLevel != newLogLevel)
        {
            logLevelSwitch.MinimumLevel = newLogLevel;
            _logger.Information("Dynamic log level changed. Debug Enabled: {DebugLevelEnabled}",
                newLogLevel == LogEventLevel.Debug);
        }

        _monitoredProperties = settings.Properties;

        if (_logger.IsEnabled(LogEventLevel.Debug))
        {
            _logger.Debug("Dynamic log level properties updated. MonitoredProperties: {@MonitoredProperties}",
                _monitoredProperties);
        }
        else
        {
            _logger.Information("Dynamic log level properties updated");
        }
    }

    public void Dispose() => _propertiesChangedHandler?.Dispose();
}
