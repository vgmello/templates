// PrometheusMetrics.cs
namespace SampleApp
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Prometheus;

    public class PrometheusMetrics
    {
        public static void PrometheusIntegration()
        {
            var builder = Host.CreateApplicationBuilder();
            builder.Services.AddHealthChecks();
            var app = builder.Build();

            // <PrometheusIntegration>
            app.UseMetricServer();
            app.MapHealthChecks("/healthz", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = Prometheus.Metrics.CreatePrometheusResponseWriter()
            });
            // </PrometheusIntegration>

            app.Run();
        }
    }
}