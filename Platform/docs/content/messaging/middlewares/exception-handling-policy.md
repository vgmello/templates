---
title: Exception Handling Policy
description: Learn how the ExceptionHandlingPolicy ensures robust error handling in your Wolverine message processing pipelines.
---

# Exception Handling Policy

The `ExceptionHandlingPolicy` is a Wolverine handler policy that automatically injects an `ExceptionHandlingFrame` into every message handling chain. This policy guarantees that all message processing logic is wrapped in a `try-catch` block, providing a consistent and centralized mechanism for handling exceptions within your messaging infrastructure.

## Purpose

The primary goal of this policy is to enforce a standard approach to error handling for all message consumers. By applying this policy, you ensure that:

-   **Consistent Error Capture**: All exceptions thrown during message handling are caught, preventing unhandled exceptions from crashing your application or leaving messages in an unprocessable state.
-   **Enhanced Observability**: Exceptions are recorded and made available through the `Envelope.Failure` property, which can be used by logging, monitoring, and error reporting systems.
-   **Reliable Message Processing**: Even if a message handler fails, the messaging pipeline can gracefully handle the error, potentially moving the message to an error queue or triggering retry mechanisms.

## How it works

When the `ExceptionHandlingPolicy` is applied to Wolverine, it iterates through all defined message `HandlerChain`s. For each chain, it checks if an `ExceptionHandlingFrame` is already present in its middleware collection. If not, it adds a new `ExceptionHandlingFrame` to the chain.

The `ExceptionHandlingFrame` (as documented separately) is responsible for generating the actual `try-catch` block around the message handler's code, ensuring that exceptions are caught and the `Envelope.Failure` property is populated.

## Usage example

To enable the `ExceptionHandlingPolicy`, you need to add it to your Wolverine options during application startup. This is typically done in your `Program.cs` file:

```csharp
// In Program.cs or a Wolverine configuration file
builder.Services.AddWolverine(opts =>
{
    opts.Policies.Add<ExceptionHandlingPolicy>();
});
```

Once applied, every message handler in your application will automatically benefit from this robust exception handling mechanism.

## See also

- [Exception Handling Frame](./exception-handling-frame.md)
- [Wolverine Setup Extensions](../wolverine-setup.md)
