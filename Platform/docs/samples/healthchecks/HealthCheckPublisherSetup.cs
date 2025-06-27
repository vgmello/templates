using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

public class HealthCheckPublisherSetup
{
    public static void ConfigurePublisher()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddHealthChecks();

        // Register a custom health check publisher
        builder.Services.AddSingleton<IHealthCheckPublisher, MyCustomHealthCheckPublisher>();

        var app = builder.Build();
        app.Run();
    }
}

public class MyCustomHealthCheckPublisher : IHealthCheckPublisher
{
    public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Health Report Status: {report.Status}");
        foreach (var entry in report.Entries)
        {
            Console.WriteLine($"  {entry.Key}: {entry.Value.Status}");
        }
        return Task.CompletedTask;
    }
}