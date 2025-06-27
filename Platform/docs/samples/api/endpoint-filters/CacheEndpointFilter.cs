// CacheEndpointFilter.cs
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

public class CacheEndpointFilter : IEndpointFilter
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheEndpointFilter> _logger;

    public CacheEndpointFilter(
        IDistributedCache cache,
        ILogger<CacheEndpointFilter> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, 
        EndpointFilterDelegate next)
    {
        var request = context.HttpContext.Request;
        
        if (!HttpMethods.IsGet(request.Method))
        {
            return await next(context);
        }

        var cacheKey = GenerateCacheKey(request);
        var cachedResponse = await _cache.GetStringAsync(cacheKey);
        
        if (cachedResponse != null)
        {
            _logger.LogInformation("Cache hit for key: {CacheKey}", cacheKey);
            return Results.Json(JsonSerializer.Deserialize<object>(cachedResponse));
        }

        var result = await next(context);
        
        if (result is IResult okResult)
        {
            // This part is tricky as IResult doesn't directly expose content for serialization.
            // For a real scenario, you'd need to capture the response body.
            // For demonstration, we'll just simulate caching.
            var serialized = "{}"; // Placeholder
            await _cache.SetStringAsync(cacheKey, serialized, TimeSpan.FromMinutes(15));
            _logger.LogInformation("Cached response for key: {CacheKey}", cacheKey);
        }

        return result;
    }

    private static string GenerateCacheKey(HttpRequest request)
    {
        return $"endpoint_cache:{request.Path}{request.QueryString}";
    }
}
