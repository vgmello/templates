// CustomPublishers.cs
namespace SampleApp
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Hosting;
    using System.Threading;
    using System.Threading.Tasks;

    public class CustomPublishers
    {
        public static void CustomPublisher()
        {
            var builder = Host.CreateApplicationBuilder();

            // <CustomPublisher>
            builder.Services.AddHealthChecks()
                .AddCheck<CustomHealthCheck>("Custom Check");

            builder.Services.AddSingleton<IHealthCheckPublisher, CustomHealthCheckPublisher>();
            // </CustomPublisher>

            var app = builder.Build();
            app.Run();
        }
    }

    public class CustomHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HealthCheckResult.Healthy("Custom health check is healthy."));
        }
    }

    public class CustomHealthCheckPublisher : IHealthCheckPublisher
    {
        public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Custom Health Report: {report.Status}");
            return Task.CompletedTask;
        }
    }
}