// MicroserviceDependencies.cs
namespace SampleApp
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class MicroserviceDependencies
    {
        public static void ServiceDependencies()
        {
            var builder = Host.CreateApplicationBuilder();

            // <ServiceDependencies>
            builder.Services.AddHealthChecks()
                .AddCheck<UserServiceHealthCheck>("User Service");
            // </ServiceDependencies>

            var app = builder.Build();
            app.Run();
        }
    }

    public class UserServiceHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            // Logic to check user service health
            return Task.FromResult(HealthCheckResult.Healthy("User service is healthy."));
        }
    }
}