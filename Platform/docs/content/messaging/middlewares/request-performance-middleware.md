---
title: Request Performance Middleware
description: Monitor the performance of your Wolverine message processing with the RequestPerformanceMiddleware, tracking execution times and errors.
---

# Request Performance Middleware

The `RequestPerformanceMiddleware` is a Wolverine middleware designed to track and log the performance of message processing. It measures the time taken to process each message, logs the start and completion (or failure) of message handling, and integrates with `MessagingMetrics` for aggregated performance data.

## Purpose

This middleware provides valuable insights into the efficiency and reliability of your message consumers by:

-   **Measuring Execution Time**: Records the time elapsed from when a message is received until its processing is complete.
-   **Logging Performance**: Logs detailed information about message processing, including the message type, execution time, and any failures.
-   **Metric Integration**: Publishes performance metrics to `MessagingMetrics`, allowing for aggregation and visualization of message processing performance over time.
-   **Error Tracking**: Records exceptions that occur during message processing, contributing to a better understanding of system health.

## How it works

The `RequestPerformanceMiddleware` uses two main methods that Wolverine calls at different stages of message processing:

### Before

The `Before` method is executed when a message is first received by the Wolverine pipeline. It performs the following actions:

1.  **Records Start Time**: Captures a high-resolution timestamp to mark the beginning of message processing.
2.  **Logs Request**: Logs a trace-level message indicating that the message has been received, including the message type and its content.
3.  **Initializes Metrics**: Retrieves or creates `MessagingMetrics` for the specific message type and increments the `MessageReceived` counter.

### Finally

The `Finally` method is executed after the message handler has completed, regardless of whether it succeeded or failed. It performs the following actions:

1.  **Calculates Elapsed Time**: Determines the total time taken for message processing.
2.  **Records Processing Time**: Publishes the elapsed time to `MessagingMetrics`.
3.  **Logs Completion/Failure**: Logs a debug-level message if the message processing was successful, indicating the message type and execution time. If an exception occurred, it logs an error-level message with the exception details and updates `MessagingMetrics` to record the exception.

## Usage example

To enable the `RequestPerformanceMiddleware`, you need to add it to your Wolverine policies. This is typically done during Wolverine configuration in your `Program.cs`:

```csharp
// In Program.cs or a Wolverine configuration file
builder.Services.AddWolverine(opts =>
{
    opts.Policies.AddMiddleware<RequestPerformanceMiddleware>();
});
```

For the metrics to be collected and exposed, ensure you have configured OpenTelemetry metrics in your application and are exporting them to a compatible backend.

## See also

- [Messaging Metrics](../telemetry/messaging-metrics.md)
- [Messaging Meter Store](../telemetry/messaging-meter-store.md)
- [Wolverine Setup Extensions](../wolverine-setup.md)
