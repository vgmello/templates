// ConditionalLoggingFilter.cs
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

public class ConditionalLoggingFilter : IEndpointFilter
{
    private readonly ILogger<ConditionalLoggingFilter> _logger;
    private readonly IConfiguration _configuration;

    public ConditionalLoggingFilter(
        ILogger<ConditionalLoggingFilter> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, 
        EndpointFilterDelegate next)
    {
        var enableLogging = _configuration.GetValue<bool>("Features:DetailedLogging");
        
        if (enableLogging)
        {
            _logger.LogInformation("Executing endpoint with detailed logging");
        }

        return await next(context);
    }
}
