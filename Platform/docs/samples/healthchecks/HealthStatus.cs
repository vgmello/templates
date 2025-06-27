// HealthStatus.cs
namespace SampleApp
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Operations.ServiceDefaults;

    public class HealthStatus
    {
        public static void HealthEndpoint()
        {
            var builder = Host.CreateApplicationBuilder();
            builder.AddServiceDefaults();
            var app = builder.Build();

            // <HealthEndpoint>
            app.MapHealthChecks("/health");
            // </HealthEndpoint>

            app.Run();
        }
    }
}