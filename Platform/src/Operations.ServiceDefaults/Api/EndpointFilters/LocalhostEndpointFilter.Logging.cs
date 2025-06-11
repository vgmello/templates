using System.Net;
using Microsoft.Extensions.Logging;

namespace Operations.ServiceDefaults.Api.EndpointFilters;

internal static partial class LocalhostEndpointFilterLoggerExtensions
{
    public static void LogRemoteRequestForLocalEndpoint(this ILogger logger, LogLevel level, IPAddress? remoteIpAddress)
    {
        LogRemoteRequestForLocalEndpointImpl(logger, level, remoteIpAddress);
    }

    [LoggerMessage(EventId = 1,
        Message = "Remote request received for a local-only endpoint, returning unauthorized. IP address: {RemoteIpAddress}")]
    private static partial void LogRemoteRequestForLocalEndpointImpl(ILogger logger, LogLevel level, IPAddress? remoteIpAddress);
}
