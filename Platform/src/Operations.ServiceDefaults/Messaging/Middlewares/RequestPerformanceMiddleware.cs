// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.Extensions.Logging;
using Operations.ServiceDefaults.Messaging.Telemetry;
using Operations.ServiceDefaults.Messaging.Wolverine;
using System.Diagnostics;
using Wolverine;

namespace Operations.ServiceDefaults.Messaging.Middlewares;

public partial class RequestPerformanceMiddleware
{
    private string _messageTypeName = string.Empty;
    private long _startedTime;
    private MessagingMetrics? _messagingMetrics;

    public void Before(ILogger logger, Envelope envelope, MessagingMeterStore meterStore)
    {
        _messageTypeName = envelope.GetMessageName();
        _startedTime = Stopwatch.GetTimestamp();

        LogRequestReceived(logger, _messageTypeName, envelope.Message);

        var metricName = envelope.GetMessageName(fullName: true);
        _messagingMetrics = meterStore.GetOrCreateMetrics(metricName);

        _messagingMetrics.MessageReceived();
    }

    public void Finally(ILogger logger, Envelope envelope)
    {
        var elapsedTime = Stopwatch.GetElapsedTime(_startedTime);
        _messagingMetrics?.RecordProcessingTime(elapsedTime);

        if (envelope.Failure is null)
        {
            LogRequestCompleted(logger, _messageTypeName, elapsedTime);
        }
        else
        {
            LogRequestFailed(logger, envelope.Failure, _messageTypeName, elapsedTime);
            _messagingMetrics?.ExceptionHappened(envelope.Failure);
        }
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Trace,
        Message = "{MessageType} received. Message: {@Message}")]
    private static partial void LogRequestReceived(ILogger logger, string messageType, object? message);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Debug,
        Message = "{MessageType} completed in {MessageExecutionTime}")]
    private static partial void LogRequestCompleted(ILogger logger, string messageType, TimeSpan messageExecutionTime);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "{MessageType} failed after {MessageExecutionTime}")]
    private static partial void LogRequestFailed(ILogger logger, Exception ex, string messageType, TimeSpan messageExecutionTime);
}
