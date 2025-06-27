// RateLimitEndpointFilter.cs
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;

public class RateLimitEndpointFilter : IEndpointFilter
{
    private readonly IMemoryCache _cache;
    private readonly int _maxRequests;
    private readonly TimeSpan _window;

    public RateLimitEndpointFilter(
        IMemoryCache cache, 
        int maxRequests = 100, 
        TimeSpan? window = null)
    {
        _cache = cache;
        _maxRequests = maxRequests;
        _window = window ?? TimeSpan.FromMinutes(1);
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, 
        EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var clientId = GetClientIdentifier(httpContext);
        var key = $"rate_limit:{clientId}";

        var requestCount = _cache.Get<int>(key);
        
        if (requestCount >= _maxRequests)
        {
            httpContext.Response.Headers.Add("Retry-After", 
                _window.TotalSeconds.ToString(CultureInfo.InvariantCulture));
            
            return Results.Problem(
                statusCode: 429,
                title: "Too Many Requests",
                detail: $"Rate limit exceeded. Maximum {_maxRequests} requests per {_window}.");
        }

        _cache.Set(key, requestCount + 1, _window);
        
        return await next(context);
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        return context.User.Identity?.IsAuthenticated == true
            ? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous"
            : context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
