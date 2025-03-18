// Copyright (c) ABCDEG.All rights reserved.

using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Wolverine;

namespace Operations.ServiceDefaults.Messaging.Behaviors;

public static partial class RequestPerformanceBehavior
{
    public static long Before(ILogger logger, Envelope envelope)
    {
        var startedTime = Stopwatch.GetTimestamp();

        logger.LogDebug("AAA Request received: {MessageType} {@Message}", GetMessageTypeName(envelope),
            envelope.Message);

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
