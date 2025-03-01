// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.Extensions.Options;
using Serilog.Core;
using Serilog.Events;

namespace Operations.ServiceDefaults.Logging;

public class DynamicPropertyLogFilter(IOptionsMonitor<MonitoredLogProperties> monitoredPropConfig, Logger standardLogger) : ILogEventFilter
{
    public bool IsEnabled(LogEvent logEvent)
    {
        var logProperties = monitoredPropConfig.CurrentValue;

        if (logProperties.Count == 0 || standardLogger.IsEnabled(logEvent.Level))
            return false;

        foreach (var property in logEvent.Properties)
        {
            if (logProperties.TryGetValue(property.Key, out var values) && values.Contains(property.Value.ToString()))
                return true;
        }

        return false;
    }
}
