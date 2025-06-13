// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using Wolverine;

namespace Operations.ServiceDefaults.Messaging.Middlewares;

// Rule: A value being logged doesn't have an effective way to be converted into a string
// Reason: Middleware log message
#pragma warning disable LOGGEN036

public static partial class RequestPerformanceMiddleware
{
    internal record struct MetricNameParts(string Prefix, string CommandName);

    internal static MetricNameParts ExtractMetricParts(string fullTypeName)
    {
        if (string.IsNullOrEmpty(fullTypeName))
        {
            return new MetricNameParts("unknown", "unknown_type");
        }

        var typeName = fullTypeName;
        string namespaceStr = string.Empty;

        int lastDotIndex = fullTypeName.LastIndexOf('.');
        if (lastDotIndex > 0 && lastDotIndex < fullTypeName.Length - 1)
        {
            typeName = fullTypeName.Substring(lastDotIndex + 1);
            namespaceStr = fullTypeName.Substring(0, lastDotIndex);
        }
        else // No namespace, or malformed
        {
            // If fullTypeName itself doesn't have a dot, it's likely just the class name.
            // Or it could be an ID if GetMessageTypeName fell back to envelope.Id.ToString()
            if (Guid.TryParse(fullTypeName, out _)) // Check if it's a GUID (from envelope.Id)
            {
                return new MetricNameParts("system", "message_id");
            }
            // Otherwise, assume it's a simple type name without namespace
        }

        var commandNamePart = typeName.ToLowerInvariant();

        var namespaceParts = namespaceStr.Split(new[] { '.' }, System.StringSplitOptions.RemoveEmptyEntries);
        var prefixPart = string.Join(".", namespaceParts.Take(2)).ToLowerInvariant();

        return new MetricNameParts(prefixPart, commandNamePart);
    }

    public static long Before(ILogger logger, Envelope envelope, Meter meter)
    {
        var startedTime = Stopwatch.GetTimestamp();
        // Use FullName for GetMessageTypeName to get namespace information
        var originalMessageTypeName = GetMessageTypeName(envelope, useFullName: true);
        LogRequestReceived(logger, originalMessageTypeName, envelope.Message);

        var metricParts = ExtractMetricParts(originalMessageTypeName);
        var metricBaseName = string.IsNullOrEmpty(metricParts.Prefix)
            ? metricParts.CommandName
            : $"{metricParts.Prefix}.{metricParts.CommandName}";

        var counter = meter.CreateCounter<long>($"{metricBaseName}.count", unit: "invocations", description: "Number of times the command has been invoked.");
        counter.Add(1);

        return startedTime;
    }

    public static void Finally(long startedTime, ILogger logger, Envelope envelope, Meter meter)
    {
        var elapsedTime = Stopwatch.GetElapsedTime(startedTime);
        // Use FullName for GetMessageTypeName
        var originalMessageTypeName = GetMessageTypeName(envelope, useFullName: true);

        LogRequestCompleted(logger, originalMessageTypeName, elapsedTime.TotalMilliseconds);

        var metricParts = ExtractMetricParts(originalMessageTypeName);
        var metricBaseName = string.IsNullOrEmpty(metricParts.Prefix)
            ? metricParts.CommandName
            : $"{metricParts.Prefix}.{metricParts.CommandName}";

        var histogram = meter.CreateHistogram<double>($"{metricBaseName}.duration", unit: "ms", description: "Execution duration of the command.");
        histogram.Record(elapsedTime.TotalMilliseconds);
    }

    public static void OnException(long startedTime, ILogger logger, Envelope envelope, Exception ex, Meter meter)
    {
        // Get full message type name for consistent metric tagging
        var originalMessageTypeName = GetMessageTypeName(envelope, useFullName: true);

        // Log the exception
        LogRequestFailed(logger, ex, originalMessageTypeName, Stopwatch.GetElapsedTime(startedTime).TotalMilliseconds);

        // Record exception metric
        var metricParts = ExtractMetricParts(originalMessageTypeName);
        var metricBaseName = string.IsNullOrEmpty(metricParts.Prefix)
            ? metricParts.CommandName
            : $"{metricParts.Prefix}.{metricParts.CommandName}";

        var exceptionCounter = meter.CreateCounter<long>($"{metricBaseName}.exceptions", unit: "exceptions", description: "Number of times the command processing resulted in an exception.");
        exceptionCounter.Add(1);
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "{MessageType} received. Message: {@Message}")]
    private static partial void LogRequestReceived(ILogger logger, string messageType, object? message);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Debug,
        Message = "{MessageType} completed in {MessageExecutionTime:000} ms")]
    private static partial void LogRequestCompleted(ILogger logger, string messageType, double messageExecutionTime);

    [LoggerMessage(
        EventId = 3, // New EventId
        Level = LogLevel.Error, // Appropriate level for exceptions
        Message = "{MessageType} failed after {MessageExecutionTime:000} ms.")]
    private static partial void LogRequestFailed(ILogger logger, Exception ex, string messageType, double messageExecutionTime);

    // Updated to allow choosing between Name and FullName
    private static string GetMessageTypeName(Envelope envelope, bool useFullName = false)
    {
        if (envelope.Message?.GetType() != null)
        {
            return useFullName ? envelope.Message.GetType().FullName : envelope.Message.GetType().Name;
        }
        return envelope.MessageType ?? envelope.Id.ToString(); // MessageType is often FQN if available
    }
}
