// Copyright (c) ABCDEG. All rights reserved.

using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Operations.ServiceDefaults.Api.EndpointFilters;

public partial class LocalhostEndpointFilter(ILogger logger) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var remoteIp = context.HttpContext.Connection.RemoteIpAddress;

        if (remoteIp is null || !IPAddress.IsLoopback(remoteIp))
        {
            logger.LogRemoteRequestForLocalEndpoint(LogLevel.Debug, remoteIp);

            return Results.Unauthorized();
        }

        return await next(context);
    }

}
