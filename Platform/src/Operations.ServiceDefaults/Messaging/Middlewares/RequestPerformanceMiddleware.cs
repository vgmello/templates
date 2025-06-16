// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Wolverine;

namespace Operations.ServiceDefaults.Messaging.Middlewares;

// Rule: A value being logged doesn't have an effective way to be converted into a string
// Reason: Middleware log message
#pragma warning disable LOGGEN036

public static partial class RequestPerformanceMiddleware
{
    public static long Before(ILogger logger, Envelope envelope)
    {
        var startedTime = Stopwatch.GetTimestamp();

        LogRequestReceived(logger, GetMessageTypeName(envelope), envelope.Message);

        return startedTime;
    }

    public static void Finally(long startedTime, ILogger logger, Envelope envelope)
    {
        var elapsedTime = Stopwatch.GetElapsedTime(startedTime);

        LogRequestCompleted(logger, GetMessageTypeName(envelope), elapsedTime.TotalMilliseconds);
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

    private static string GetMessageTypeName(Envelope envelope) =>
        envelope.Message?.GetType().Name ?? envelope.MessageType ?? envelope.Id.ToString();
}
