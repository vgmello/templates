// PerformanceOptimization.cs
namespace SampleApp
{
    using Microsoft.Extensions.Hosting;

    public class PerformanceOptimization
    {
        public static void OptimizationTechniques()
        {
            var builder = Host.CreateApplicationBuilder();
            // <OptimizationTechniques>
            // Optimization techniques:
            // - Cache health check results for a short period
            // - Run expensive checks less frequently
            // - Use asynchronous health checks
            // - Optimize underlying dependencies
            // </OptimizationTechniques>
            var app = builder.Build();
            app.Run();
        }
    }
}