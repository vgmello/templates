---
title: Health Check Status Store
description: Understand the HealthCheckStatusStore, a simple in-memory store for your application's last known health status.
---

# Health Check Status Store

The `HealthCheckStatusStore` is a straightforward, in-memory component designed to hold the last reported health status of your application. It serves as a quick reference for liveness probes and other internal checks that need to know the application's state without re-executing all health checks.

## Purpose

The primary purpose of this store is to provide a readily available, up-to-date health status. This is particularly useful for:

-   **Liveness Probes**: Endpoints like `/status` can quickly return the last known health status without incurring the overhead of running all registered health checks.
-   **Internal Monitoring**: Other parts of your application can query this store to react to changes in the overall health.

## How it works

The `HealthCheckStatusStore` contains a single property, `LastHealthStatus`, which is of type `HealthStatus` (an enum with values like `Healthy`, `Degraded`, `Unhealthy`). This property is updated whenever a health check report is processed, typically by the `MapDefaultHealthCheckEndpoints` extension.

By default, the `LastHealthStatus` is initialized to `HealthStatus.Healthy`.

## Usage example

You typically don't interact with `HealthCheckStatusStore` directly in your application code unless you have a specific need to query or update the last health status outside of the standard health check pipeline.

It is automatically registered and used by the `MapDefaultHealthCheckEndpoints` extension. If you need to access it, you can retrieve it from the dependency injection container:

```csharp
// In Program.cs or any service that needs to access the health status
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Operations.ServiceDefaults.HealthChecks;

public class MyService
{
    private readonly HealthCheckStatusStore _statusStore;

    public MyService(HealthCheckStatusStore statusStore)
    {
        _statusStore = statusStore;
    }

    public void CheckApplicationStatus()
    {
        if (_statusStore.LastHealthStatus == HealthStatus.Unhealthy)
        {
            Console.WriteLine("Application is unhealthy!");
            // Take corrective action
        }
    }
}
```

## See also

- [Health Check Setup Extensions](./setup.md)
