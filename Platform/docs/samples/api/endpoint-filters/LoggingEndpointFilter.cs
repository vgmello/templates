// LoggingEndpointFilter.cs
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

public class LoggingEndpointFilter : IEndpointFilter
{
    private readonly ILogger<LoggingEndpointFilter> _logger;

    public LoggingEndpointFilter(ILogger<LoggingEndpointFilter> logger)
    {
        _logger = logger;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, 
        EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var endpoint = httpContext.GetEndpoint()?.DisplayName;
        
        _logger.LogInformation("Executing endpoint: {Endpoint}", endpoint);
        
        var stopwatch = Stopwatch.StartNew();
        var result = await next(context);
        stopwatch.Stop();
        
        _logger.LogInformation("Endpoint {Endpoint} completed in {ElapsedMs}ms", 
            endpoint, stopwatch.ElapsedMilliseconds);
        
        return result;
    }
}
