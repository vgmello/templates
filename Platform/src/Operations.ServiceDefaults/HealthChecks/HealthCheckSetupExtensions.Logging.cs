using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Operations.ServiceDefaults.HealthChecks;

internal static partial class HealthCheckLoggingExtensions
{
    public static void LogHealthCheckResponseHealthy(this ILogger logger, LogLevel level, HealthReport report)
    {
        LogHealthCheckResponseHealthyImpl(logger, level, report);
    }

    public static void LogHealthCheckFailedWarning(this ILogger logger, LogLevel level, object failedHealthReport)
    {
        LogHealthCheckFailedWarningImpl(logger, level, failedHealthReport);
    }

    public static void LogHealthCheckFailedError(this ILogger logger, LogLevel level, object failedHealthReport)
    {
        LogHealthCheckFailedErrorImpl(logger, level, failedHealthReport);
    }

    [LoggerMessage(EventId = 1, Message = "Health check response: {@HealthReport}")]
    private static partial void LogHealthCheckResponseHealthyImpl(ILogger logger, LogLevel level, HealthReport report);

    [LoggerMessage(EventId = 2, Message = "Health check failed: {FailedHealthReport}")]
    private static partial void LogHealthCheckFailedWarningImpl(ILogger logger, LogLevel level, object failedHealthReport);

    [LoggerMessage(EventId = 3, Message = "Health check failed: {FailedHealthReport}")]
    private static partial void LogHealthCheckFailedErrorImpl(ILogger logger, LogLevel level, object failedHealthReport);
}
