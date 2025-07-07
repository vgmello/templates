// Metrics.cs
namespace SampleApp
{
    using Microsoft.Extensions.Hosting;
    using System.Diagnostics.Metrics;

    public class Metrics
    {
        public static void CollectMetrics()
        {
            var builder = Host.CreateApplicationBuilder();

            // <Metrics>
            var meter = new Meter("SampleApp.Metrics");
            var counter = meter.CreateCounter<long>("requests-total");
            counter.Add(1);
            // </Metrics>

            var app = builder.Build();
            app.Run();
        }
    }
}