// Copyright (c) ABCDEG. All rights reserved.

using Serilog.Core;
using Serilog.Events;

namespace Operations.ServiceDefaults.Logging;

public class DynamicPropertyLogFilter(MonitoredLogProperties monitoredLogProperties, Logger standardLogger) : ILogEventFilter
{
    public bool IsEnabled(LogEvent logEvent)
    {
        if (monitoredLogProperties.Count == 0 || standardLogger.IsEnabled(logEvent.Level))
            return false;

        foreach (var property in logEvent.Properties)
        {
            if (monitoredLogProperties.TryGetValue(property.Key, out var values) && values.Contains(property.Value.ToString()))
                return true;
        }

        return false;
    }
}
