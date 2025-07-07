// TimeoutEndpointFilter.cs
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

public class TimeoutEndpointFilter : IEndpointFilter
{
    private readonly TimeSpan _timeout;

    public TimeoutEndpointFilter(TimeSpan timeout)
    {
        _timeout = timeout;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, 
        EndpointFilterDelegate next)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(
            context.HttpContext.RequestAborted);
        
        cts.CancelAfter(_timeout);

        try
        {
            return await next(context);
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
        {
            return Results.Problem(
                statusCode: 408,
                title: "Request Timeout",
                detail: $"Request exceeded the maximum allowed time of {_timeout}.");
        }
    }
}
