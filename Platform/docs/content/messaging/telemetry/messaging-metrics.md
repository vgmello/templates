---
title: Messaging Metrics
description: Explore the MessagingMetrics class, which defines and records key OpenTelemetry metrics for message processing performance and error tracking.
---

# Messaging Metrics

The `MessagingMetrics` class is responsible for defining and recording specific OpenTelemetry metrics related to the processing of individual message types. It provides a structured way to capture performance and error data, which can then be exported to observability platforms for analysis and visualization.

## Purpose

This class encapsulates the creation and updating of three core metrics for each message type:

-   **`{metricName}.count`**: Tracks the total number of times a message (command/query) has been invoked or received.
-   **`{metricName}.duration`**: Measures the execution duration of the message processing, providing insights into latency.
-   **`{metricName}.exceptions`**: Counts the number of times message processing resulted in an exception, indicating potential issues.

By centralizing these metrics, `MessagingMetrics` ensures consistency in data collection and simplifies the process of monitoring message-driven workflows.

## How it works

`MessagingMetrics` is instantiated with a `metricName` (derived from the message type) and an OpenTelemetry `Meter`. It then creates three specific instruments:

-   **`_messagesReceived` (Counter)**: Incremented each time a message is received.
-   **`_messageProcessingTime` (Histogram)**: Records the duration of message processing in milliseconds.
-   **`_exceptionsCount` (Counter)**: Incremented when an exception occurs during message processing. It also includes a tag for the exception type.

## Key Methods

### MessageReceived()

Increments the `_messagesReceived` counter by one. Call this method when a message is initially received for processing.

```csharp
public void MessageReceived()
```

### RecordProcessingTime(TimeSpan duration)

Records the total processing time for a message. The duration is converted to milliseconds before being recorded.

```csharp
public void RecordProcessingTime(TimeSpan duration)
```

-   **`duration`**: The `TimeSpan` representing the total time taken to process the message.

### ExceptionHappened(Exception exception)

Increments the `_exceptionsCount` counter and adds a tag indicating the type of exception that occurred. Call this method when an unhandled exception is caught during message processing.

```csharp
public void ExceptionHappened(Exception exception)
```

-   **`exception`**: The `Exception` object that was caught.

## Usage example

You typically obtain an instance of `MessagingMetrics` from the `MessagingMeterStore` and then use its methods within your message processing logic (e.g., in a Wolverine middleware or message handler).

```csharp
using Operations.ServiceDefaults.Messaging.Telemetry;
using System.Diagnostics;

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

        metrics.MessageReceived();

        var stopwatch = Stopwatch.StartNew();
        try
        {
            // Simulate message processing
            Thread.Sleep(100);
            Console.WriteLine($"Processing {messageType}");
        }
        catch (Exception ex)
        {
            metrics.ExceptionHappened(ex);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            metrics.RecordProcessingTime(stopwatch.Elapsed);
        }
    }
}
```

## See also

- [Messaging Meter Store](./messaging-meter-store.md)
- [Request Performance Middleware](../middlewares/request-performance-middleware.md)
