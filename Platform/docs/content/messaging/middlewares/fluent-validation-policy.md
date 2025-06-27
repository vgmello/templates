---
title: Fluent Validation Policy
description: Learn how the FluentValidationPolicy automatically integrates FluentValidation into your Wolverine message handling pipelines.
---

# Fluent Validation Policy

The `FluentValidationPolicy` is a Wolverine handler policy that automatically applies FluentValidation to your incoming messages. This policy inspects each message handling chain, identifies the message type, and dynamically inserts validation logic using registered `IValidator<T>` instances. This ensures that messages are validated before they reach your core business logic.

## Purpose

The primary goal of this policy is to enforce data integrity and business rules early in the message processing pipeline. By integrating FluentValidation at this stage, you can:

-   **Prevent Invalid Messages**: Stop invalid messages from being processed, reducing errors and improving system reliability.
-   **Centralize Validation**: Keep your validation rules separate from your message handlers, promoting cleaner code and better separation of concerns.
-   **Consistent Validation**: Ensure that all messages of a certain type are validated consistently across your application.

## How it works

When the `FluentValidationPolicy` is applied, it iterates through each `HandlerChain` in Wolverine. For each chain, it performs the following steps:

1.  **Identifies Message Type**: It determines the type of the message being handled by the chain.
2.  **Discovers Validators**: It queries the `IServiceContainer` (Wolverine's dependency injection container) to find all registered `IValidator<T>` instances for the identified message type.
3.  **Selects Execution Method**: Based on the number of discovered validators (one or many), it selects the appropriate method from `FluentValidationExecutor` (`ExecuteOne` or `ExecuteMany`).
4.  **Inserts Middleware**: It adds a `MethodCall` to `FluentValidationExecutor` as middleware to the chain. This call executes the validation logic.
5.  **Adds Result Frame**: It also adds a `FluentValidationResultFrame` to the chain. This frame is responsible for handling the outcome of the validation (e.g., throwing an exception if validation fails).

This dynamic insertion ensures that validation occurs automatically for any message type that has a registered FluentValidation validator.

## Usage example

To enable the `FluentValidationPolicy`, you need to add it to your Wolverine options during application startup. This is typically done in your `Program.cs` file:

```csharp
// In Program.cs or a Wolverine configuration file
builder.Services.AddWolverine(opts =>
{
    opts.Policies.Add<FluentValidationPolicy>();
});
```

Once applied, any message that has a corresponding `IValidator<T>` registered in your dependency injection container will be automatically validated when processed by Wolverine.

## See also

- [FluentValidation Executor](./fluent-validation-executor.md)
- [Fluent Validation Result Frame](./fluent-validation-result-frame.md)
- [Wolverine Setup Extensions](../wolverine-setup.md)
