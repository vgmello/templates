// KubernetesIntegration.cs
namespace SampleApp
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class KubernetesIntegration
    {
        public static void K8sConfig()
        {
            var builder = Host.CreateApplicationBuilder();
            builder.Services.AddHealthChecks();
            var app = builder.Build();

            // <K8sConfig>
            app.MapHealthChecks("/healthz/liveness"); // Liveness probe
            app.MapHealthChecks("/healthz/readiness"); // Readiness probe
            // </K8sConfig>

            app.Run();
        }
    }
}