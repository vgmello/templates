---
title: Exception Handling Frame
description: Understand the ExceptionHandlingFrame, a code generation frame used by Wolverine to wrap message handling in a try-catch block.
---

# Exception Handling Frame

The `ExceptionHandlingFrame` is a specialized code generation frame used within Wolverine to automatically wrap message handling logic in a `try-catch` block. This ensures that any exceptions occurring during message processing are caught, recorded, and re-thrown, allowing for consistent error handling and reporting within the messaging pipeline.

## Purpose

The primary purpose of this frame is to provide a robust mechanism for handling exceptions in message consumers. By injecting a `try-catch` block around the message handling code, it ensures that:

-   **Exceptions are captured**: Any unhandled exceptions are caught at the message processing level.
-   **Failure is recorded**: The caught exception is assigned to the `Failure` property of the Wolverine `Envelope`, making the error details accessible for further processing (e.g., by error handling policies).
-   **Exceptions are re-thrown**: The exception is re-thrown to allow other Wolverine mechanisms (like error queues or retry policies) to take over.

## How it works

`ExceptionHandlingFrame` implements `SyncFrame`, meaning it generates synchronous code. When Wolverine builds the message handling pipeline for a specific message type, if an `ExceptionHandlingPolicy` is applied, this frame is inserted into the generated code.

During code generation, the `GenerateCode` method outputs a `try` block, followed by the code for the next frames in the pipeline (which would be your message handler logic), and then a `catch` block. Inside the `catch` block, it assigns the caught exception to the `Envelope.Failure` property and then re-throws the exception.

## Usage example

You typically don't interact directly with `ExceptionHandlingFrame`. It's an internal component of Wolverine's code generation. Its functionality is exposed through the `ExceptionHandlingPolicy`.

To enable this exception handling, you would apply the `ExceptionHandlingPolicy` to your Wolverine options:

```csharp
// In Program.cs or a Wolverine configuration file
builder.Services.AddWolverine(opts =>
{
    opts.Policies.Add<ExceptionHandlingPolicy>();
});
```

When this policy is active, Wolverine will ensure that your message handlers are wrapped with the necessary `try-catch` logic provided by `ExceptionHandlingFrame`.

## See also

- [Exception Handling Policy](./exception-handling-policy.md)
- [Wolverine Setup Extensions](../wolverine-setup.md)
