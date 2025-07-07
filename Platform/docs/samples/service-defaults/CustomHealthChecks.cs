// CustomHealthChecks.cs
namespace SampleApp
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Hosting;
    using Operations.ServiceDefaults;
    using System.Threading;
    using System.Threading.Tasks;

    public class CustomHealthChecks
    {
        public static void AddCustomHealthChecks()
        {
            var builder = Host.CreateApplicationBuilder();
            builder.AddServiceDefaults();

            // <CustomHealthChecks>
            builder.Services.AddHealthChecks()
                .AddCheck<MyCustomHealthCheck>("My Custom Check");
            // </CustomHealthChecks>

            var app = builder.Build();
            app.Run();
        }
    }

    public class MyCustomHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HealthCheckResult.Healthy("My custom health check is healthy."));
        }
    }
}