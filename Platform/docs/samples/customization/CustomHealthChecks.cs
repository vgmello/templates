using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Operations.Samples.Customization;

public class CustomHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // Your custom health check logic here
        return Task.FromResult(HealthCheckResult.Healthy());
    }
}

public static class CustomHealthCheckExtensions
{
    public static IHealthChecksBuilder AddCustomHealthCheck(this IHealthChecksBuilder builder)
    {
        return builder.AddCheck<CustomHealthCheck>("custom");
    }
}
