---
title: Messaging Telemetry
description: Understand how messaging telemetry is integrated into your services for monitoring and observability.
---

# Messaging Telemetry

Messaging telemetry provides insights into the performance and behavior of your messaging infrastructure. The Platform integrates OpenTelemetry to automatically collect traces and metrics related to message production and consumption, enabling comprehensive monitoring and troubleshooting.

## Key Telemetry Aspects

### Distributed Tracing

Distributed tracing allows you to follow the journey of a message across different services, providing a complete view of its lifecycle. This is crucial for debugging issues in distributed systems.

```csharp
using System.Diagnostics;
using Wolverine.Runtime;

public class MessageTracingExample
{
    public static void IllustrateTracing()
    {
        var activitySource = new ActivitySource("MyApplication.Messaging");

        // When a message is sent or received, an Activity is created and propagated
        using (var activity = activitySource.StartActivity("MessageSend", ActivityKind.Producer))
        {
            activity?.SetTag("message.type", "OrderCreated");
            activity?.SetTag("message.id", Guid.NewGuid());
            // ... send message
        }

        using (var activity = activitySource.StartActivity("MessageReceive", ActivityKind.Consumer))
        {
            activity?.SetTag("message.type", "OrderCreated");
            activity?.SetTag("message.id", Guid.NewGuid());
            // ... process message
        }
    }
}
```

### Metrics Collection

Metrics provide quantitative data about your messaging system, such as message rates, processing times, and error counts. This data can be used to create dashboards and alerts.

```csharp
using System.Diagnostics.Metrics;

public class MessageMetricsExample
{
    public static void IllustrateMetrics()
    {
        var meter = new Meter("MyApplication.Messaging");
        var messagesSentCounter = meter.CreateCounter<long>("messaging.messages_sent", "messages", "Number of messages sent");
        var messageProcessingTimeHistogram = meter.CreateHistogram<double>("messaging.processing_time", "ms", "Message processing time");

        // Increment counter when a message is sent
        messagesSentCounter.Add(1);

        // Record processing time when a message is processed
        messageProcessingTimeHistogram.Record(150.5); // Example: 150.5 ms
    }
}
```

## Configuration

Messaging telemetry is typically configured as part of your overall OpenTelemetry setup within the Service Defaults. You can customize the exporters and instrumentation as needed.

```csharp
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
```

## See also

*   [OpenTelemetry Overview](../../opentelemetry/overview.md)
*   [Wolverine Messaging](../wolverine-setup.md)
