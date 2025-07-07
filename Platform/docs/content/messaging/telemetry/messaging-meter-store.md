---
title: Messaging Meter Store
description: Discover the MessagingMeterStore, a central component for managing and providing messaging-related metrics for OpenTelemetry.
---

# Messaging Meter Store

The `MessagingMeterStore` is a central component responsible for managing and providing `MessagingMetrics` instances for different message types. It leverages OpenTelemetry's `Meter` to create and record custom metrics related to message processing, offering granular insights into your messaging infrastructure's performance and health.

## Purpose

This store serves as a factory and cache for `MessagingMetrics` objects. Its main purposes are:

-   **Metric Management**: Ensures that a unique `MessagingMetrics` instance exists for each distinct message type, preventing redundant metric creation.
-   **Consistent Naming**: Applies a consistent naming convention to metric names derived from message types, making it easier to query and analyze metrics in your observability platform.
-   **Performance**: Uses a `ConcurrentDictionary` to efficiently store and retrieve `MessagingMetrics` instances, minimizing overhead.

## How it works

### GetOrCreateMetrics

The core method of `MessagingMeterStore` is `GetOrCreateMetrics`. When called with a `messageType` string, it checks if a `MessagingMetrics` instance for that type already exists in its internal cache. If it does, the existing instance is returned. Otherwise, a new `MessagingMetrics` instance is created, added to the cache, and then returned.

### Metric Naming Convention

The `CreateMessagingMetrics` private method implements the logic for generating metric names. It takes the full message type name and transforms it into a snake_case format. Additionally, it removes common suffixes like `_command` or `_query` to produce cleaner, more concise metric names.

For example:

-   `MyNamespace.MyCommand` becomes `my_namespace.my`
-   `AnotherNamespace.SomeQuery` becomes `another_namespace.some`

## Usage example

You typically inject `MessagingMeterStore` into your Wolverine middlewares or message handlers where you want to record messaging-related metrics. The `RequestPerformanceMiddleware` is a prime example of a component that utilizes this store.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Operations.ServiceDefaults.Messaging.Telemetry;
using System.Diagnostics.Metrics;

public class MyMessageProcessor
{
    private readonly MessagingMeterStore _meterStore;

    public MyMessageProcessor(MessagingMeterStore meterStore)
    {
        _meterStore = meterStore;
    }

    public void ProcessMessage(string messageType)
    {
        var metrics = _meterStore.GetOrCreateMetrics(messageType);

        // Now you can use the metrics instance to record events
        metrics.MessageReceived();
        // ... process message ...
        metrics.RecordProcessingTime(TimeSpan.FromMilliseconds(100));
    }
}

// In Program.cs, ensure MessagingMeterStore is registered
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<MessagingMeterStore>();
builder.Services.AddSingleton<Meter>(sp => new Meter(MessagingMeterStore.MessagingMeterKey));
```

## See also

- [Messaging Metrics](./messaging-metrics.md)
- [Request Performance Middleware](../middlewares/request-performance-middleware.md)
