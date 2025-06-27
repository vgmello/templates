// HealthCheckTesting.cs
namespace SampleApp
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Hosting;
    using Xunit;
    using System.Threading.Tasks;

    public class HealthCheckTesting
    {
        public static void TestingChecks()
        {
            // <TestingChecks>
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddCheck<TestHealthCheck>("Test Check");

            var serviceProvider = services.BuildServiceProvider();
            var healthCheckService = serviceProvider.GetRequiredService<HealthCheckService>();

            var result = healthCheckService.CheckHealthAsync().Result;

            Assert.Equal(HealthStatus.Healthy, result.Status);
            // </TestingChecks>
        }
    }

    public class TestHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HealthCheckResult.Healthy("Test health check is healthy."));
        }
    }
}