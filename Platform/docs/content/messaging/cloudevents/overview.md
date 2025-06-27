---
title: CloudEvents Middleware
description: Learn how the CloudEventMiddleware automatically wraps integration events into CloudEvents for standardized messaging.
---

# CloudEvents Middleware

The `CloudEventMiddleware` is a Wolverine middleware that automatically transforms your application's integration events into CloudEvents. This ensures that your messages adhere to a standardized, platform-agnostic format, promoting interoperability and consistency across different systems.

## How it works

When an outgoing message is processed by Wolverine, the `CloudEventMiddleware` intercepts it. It checks if the message is an "integration event" by examining its namespace (specifically, if the namespace ends with `.IntegrationEvents`).

If the message is identified as an integration event, the middleware performs the following actions:

1.  **Creates a CloudEvent**: A new `CloudEvent` object is instantiated.
2.  **Generates ID**: A unique ID (using `Guid.CreateVersion7()`) is assigned to the CloudEvent.
3.  **Sets Type**: The `Type` of the CloudEvent is set to the name of the original message type.
4.  **Sets Source**: The `Source` of the CloudEvent is set using the `ServiceUrn` from `ServiceBusOptions`, which identifies the originating service.
5.  **Sets Timestamp**: The `Time` of the CloudEvent is set to the current UTC time.
6.  **Embeds Data**: The original message is embedded as the `Data` payload of the CloudEvent.
7.  **Sets Content Type**: The `DataContentType` is set to `application/json`, and the `ContentType` of the Wolverine envelope is set to `application/cloudevents+json`.

This process ensures that your integration events are consistently formatted as CloudEvents before being sent over the message bus.

## Usage example

To enable the `CloudEventMiddleware`, you need to add it to Wolverine's policies. This is typically done when configuring Wolverine in your `Program.cs` or a similar startup file.

If you are using the `AddWolverine` extension from `Operations.ServiceDefaults`, this middleware is already included by default.

```csharp
// In Program.cs or a Wolverine configuration file
builder.Services.AddWolverine(opts =>
{
    // ... other Wolverine configurations

    opts.Policies.AddMiddleware<CloudEventMiddleware>();
});
```

## See also

- [Wolverine Setup Extensions](../wolverine-setup.md)
- [Service Bus Options](../service-bus-options.md)
