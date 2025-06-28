---
title: OpenTelemetry Instrumentation Middleware
description: Learn how the OpenTelemetryInstrumentationMiddleware automatically instruments your Wolverine message processing with OpenTelemetry traces.
---

# OpenTelemetry Instrumentation Middleware

The `OpenTelemetryInstrumentationMiddleware` is a Wolverine middleware designed to automatically instrument your message processing with OpenTelemetry. This middleware creates and manages `Activity` objects (spans) for each message handled by Wolverine, providing valuable tracing information for distributed systems.

## Purpose

This middleware helps you gain deep visibility into your message-driven workflows by:

-   **Creating Spans**: Automatically generates a new OpenTelemetry span for each message, capturing the duration and context of its processing.
-   **Adding Tags**: Enriches spans with relevant message metadata, such as message ID, name, source, and operation type (command or query).
-   **Error Reporting**: Sets the span status to `Error` and adds error details if message processing fails, making it easy to identify and debug issues.
-   **Distributed Tracing**: Enables end-to-end distributed tracing for messages as they flow through your system, allowing you to visualize the entire journey of a message across multiple services.

## How it works

The `OpenTelemetryInstrumentationMiddleware` consists of two main static methods:

### Before

The `Before` method is executed before the message handler. It performs the following actions:

1.  **Starts an Activity**: It starts a new `Activity` (span) using a provided `ActivitySource`. The activity name is derived from the message name.
2.  **Adds Tags**: It adds various tags to the activity, including:
    -   `message.id`: The unique ID of the message envelope.
    -   `message.name`: The full name of the message type.
    -   `operation.type`: "command" if the message implements `ICommand<>`, or "query" if it implements `IQuery<>`.
    -   `message.source`: The source of the message if available in the envelope.

### Finally

The `Finally` method is executed after the message handler, regardless of whether it succeeded or failed. It performs the following actions:

1.  **Sets Status**: If the message processing was successful (no `Envelope.Failure`), the activity status is set to `Ok`. If an exception occurred (`Envelope.Failure` is not null), the status is set to `Error`, and the exception message and type are added as tags.
2.  **Stops Activity**: The activity is stopped, marking the end of the message processing span.

## Usage example

To enable OpenTelemetry instrumentation for your Wolverine messages, you need to add `OpenTelemetryInstrumentationMiddleware` to your Wolverine policies. This is typically done during Wolverine configuration in your `Program.cs`:

```csharp
// In Program.cs or a Wolverine configuration file
builder.Services.AddWolverine(opts =>
{
    opts.Policies.AddMiddleware(typeof(OpenTelemetryInstrumentationMiddleware));
});
```

For the instrumentation to be fully effective, ensure you have configured OpenTelemetry in your application, including setting up an `ActivitySource` and exporting traces to a compatible backend (e.g., Jaeger, Zipkin, OTLP).

## See also

- [OpenTelemetry Setup Extensions](../../opentelemetry/setup.md)
- [Wolverine Setup Extensions](../wolverine-setup.md)
