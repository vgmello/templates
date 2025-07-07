using Microsoft.Extensions.Diagnostics.HealthChecks;
using Operations.ServiceDefaults;
using Operations.ServiceDefaults.Api;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApiServiceDefaults();

// Register HttpClient for health checks
builder.Services.AddHttpClient();

// <ExternalServices>
// Register health checks for external service dependencies
builder.Services.AddHealthChecks()
    // HTTP endpoint health check
    .AddUrlGroup(
        uri: new Uri("https://api.external-service.com/health"),
        name: "external_api",
        configureClient: client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Add("User-Agent", "HealthCheck/1.0");
        },
        tags: new[] { "external", "api" })
    
    // Authentication service check
    .AddUrlGroup(
        uri: new Uri("https://auth.company.com/status"),
        name: "auth_service",
        tags: new[] { "external", "critical", "auth" })
    
    // Cache service check (Redis)
    .AddRedis(
        redisConnectionString: builder.Configuration.GetConnectionString("Redis")!,
        name: "redis_cache",
        tags: new[] { "external", "cache" })
    
    // Custom external service check
    .AddCheck<PaymentGatewayHealthCheck>(
        "payment_gateway",
        tags: new[] { "external", "critical", "payment" });
// </ExternalServices>

var app = builder.Build();

app.MapDefaultEndpoints();
app.Run();

// <PaymentGatewayHealthCheck>
public class PaymentGatewayHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentGatewayHealthCheck> _logger;

    public PaymentGatewayHealthCheck(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<PaymentGatewayHealthCheck> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            
            var apiKey = _configuration["PaymentGateway:ApiKey"];
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Test connectivity with a lightweight endpoint
            var response = await client.GetAsync(
                "https://api.payment-gateway.com/v1/status", 
                cancellationToken);
            
            stopwatch.Stop();

            var responseTime = stopwatch.ElapsedMilliseconds;
            var data = new Dictionary<string, object>
            {
                ["response_time_ms"] = responseTime,
                ["status_code"] = (int)response.StatusCode,
                ["endpoint"] = "status"
            };

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Payment gateway returned {StatusCode}", response.StatusCode);
                return HealthCheckResult.Degraded(
                    $"Payment gateway returned {response.StatusCode}", 
                    data: data);
            }

            if (responseTime > 5000)
            {
                return HealthCheckResult.Degraded(
                    $"Payment gateway response time is high: {responseTime}ms", 
                    data: data);
            }

            return HealthCheckResult.Healthy(
                $"Payment gateway is healthy ({responseTime}ms)", 
                data);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Payment gateway health check failed due to HTTP error");
            return HealthCheckResult.Unhealthy("Payment gateway is unreachable", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Payment gateway health check timed out");
            return HealthCheckResult.Unhealthy("Payment gateway request timed out", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment gateway health check failed");
            return HealthCheckResult.Unhealthy("Payment gateway check failed", ex);
        }
    }
}
// </PaymentGatewayHealthCheck>