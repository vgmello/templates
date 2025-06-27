using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

public class CustomHealthCheckRegistration
{
    public static void RegisterCustomCheck()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddHealthChecks()
            .AddCheck<MyCustomHealthCheck>("MyCustomCheck");

        var app = builder.Build();
        app.Run();
    }
}

public class MyCustomHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // Implement your custom health check logic here
        bool isHealthy = true; // Replace with actual logic

        if (isHealthy)
        {
            return Task.FromResult(HealthCheckResult.Healthy("My custom check is healthy."));
        }
        else
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("My custom check is unhealthy."));
        }
    }
}