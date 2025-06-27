using Microsoft.Extensions.Hosting;
using Operations.ServiceDefaults;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

public class TelemetryConfiguration
{
    public static void ConfigureTelemetry()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.AddServiceDefaults();

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing.AddSource("MyApplication.Messaging");
                // Add other tracing configurations
            })
            .WithMetrics(metrics =>
            {
                metrics.AddMeter("MyApplication.Messaging");
                // Add other metrics configurations
            });

        var app = builder.Build();
        app.Run();
    }
}