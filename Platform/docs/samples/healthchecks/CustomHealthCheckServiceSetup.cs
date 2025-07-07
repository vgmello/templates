using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System;

public class CustomHealthCheckServiceSetup
{
    public static void ConfigureHealthCheckService()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddHealthChecks();

        // Configure the HealthCheckService options
        builder.Services.Configure<HealthCheckServiceOptions>(options =>
        {
            options.Delay = TimeSpan.FromSeconds(5); // Delay before the first health check execution
            options.Period = TimeSpan.FromSeconds(30); // Period between health check executions
        });

        var app = builder.Build();
        app.Run();
    }
}