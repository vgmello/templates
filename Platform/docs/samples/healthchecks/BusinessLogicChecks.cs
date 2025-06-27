// BusinessLogicChecks.cs
namespace SampleApp
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Hosting;

    public class BusinessLogicChecks
    {
        public static void BusinessChecks()
        {
            var builder = Host.CreateApplicationBuilder();

            // <BusinessChecks>
            builder.Services.AddHealthChecks()
                .AddCheck<CustomBusinessHealthCheck>("Custom Business Logic");
            // </BusinessChecks>

            var app = builder.Build();
            app.Run();
        }
    }

    public class CustomBusinessHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            // Implement your custom business logic health check here
            var isHealthy = true; // Replace with actual logic
            if (isHealthy)
            {
                return Task.FromResult(HealthCheckResult.Healthy("Business logic is healthy."));
            }
            return Task.FromResult(HealthCheckResult.Unhealthy("Business logic is unhealthy."));
        }
    }
}