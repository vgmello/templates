---
title: Health Check Setup
description: Advanced configuration options for setting up health checks in your application.
---

# Health Check Setup

This document provides advanced configuration options for setting up health checks in your application, building upon the basic setup provided by the Platform's Service Defaults.

## Customizing Health Check Services

You can customize the `IHealthCheckService` registration to control various aspects of health check behavior, such as the delay before the first check, and the period between checks.

```csharp
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
```

## Registering Custom Health Checks

Beyond the built-in health checks, you can register your own custom health checks by implementing the `IHealthCheck` interface.

```csharp
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
```

## Health Check Publishers

Health check publishers allow you to send health check results to external systems or logs. You can implement custom publishers or use existing ones like the Application Insights publisher.

```csharp
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
```

## See also

*   [Health Checks Overview](overview.md)