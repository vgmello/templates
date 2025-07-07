---
title: Fluent Validation Result Frame
description: Understand the FluentValidationResultFrame, a Wolverine code generation frame that processes validation results and short-circuits message handling on failure.
---

# Fluent Validation Result Frame

The `FluentValidationResultFrame` is a Wolverine code generation frame responsible for evaluating the outcome of FluentValidation. If validation failures are present, this frame can short-circuit the message handling pipeline, preventing invalid messages from proceeding to the core business logic. It also ensures that validation results are properly propagated as cascading messages.

## Purpose

This frame plays a crucial role in maintaining data integrity within the messaging system. Its main purposes are:

-   **Short-circuiting on Failure**: If validation errors are found, it stops the execution of the current message handler chain.
-   **Result Propagation**: It transforms validation failures into a `Result<T>` type and publishes them as cascading messages, allowing other parts of the system to react to validation outcomes.
-   **Integration with Commands/Queries**: It specifically looks for message types that implement `ICommand<Result<T>>` or `IQuery<Result<T>>` to correctly handle their response types.

## How it works

The `FluentValidationResultFrame` is inserted into the Wolverine message handling pipeline by the `FluentValidationPolicy`. After the `FluentValidationExecutor` has run and potentially produced validation failures, this frame takes over.

1.  **Checks for Validation Failures**: It inspects the result of the validation method call. If there are any `ValidationFailure` instances, it proceeds to short-circuit.
2.  **Determines Response Type**: It identifies if the message being handled is a command or query that is expected to return a `Result<T>`.
3.  **Casts Validation Result**: If a `Result<T>` response type is detected, it casts the validation failures into an appropriate `Result<T>` object (e.g., `Result.Failure<T>(validationErrors)`).
4.  **Cascading Message**: This `Result<T>` object is then published as a cascading message using Wolverine's `CaptureCascadingMessages` mechanism. This means the validation result itself becomes a message that can be handled by other parts of your system (e.g., to log errors, send notifications, or return a problem detail to an API caller).
5.  **Short-Circuits**: After publishing the cascading message, the frame returns, effectively stopping the execution of subsequent frames in the current message handling chain.

If no validation failures are present, the frame simply allows the pipeline to continue to the next frame (typically the actual message handler).

## Usage example

You typically don't interact directly with `FluentValidationResultFrame`. Its functionality is managed by the `FluentValidationPolicy`.

To ensure this frame is active, you need to apply the `FluentValidationPolicy` to your Wolverine options:

```csharp
// In Program.cs or a Wolverine configuration file
builder.Services.AddWolverine(opts =>
{
    opts.Policies.Add<FluentValidationPolicy>();
});
```

When a message like the following is processed:

```csharp
// Example Command
public record CreateUserCommand(string Username, string Email) : ICommand<Result<Guid>>;

// Example Validator
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Email).EmailAddress();
    }
}
```

If `CreateUserCommand` fails validation, the `FluentValidationResultFrame` will ensure that a `Result<Guid>` representing the validation failure is returned and potentially handled as a cascading message, preventing the command handler from executing.

## See also

- [Fluent Validation Policy](./fluent-validation-policy.md)
- [FluentValidation Executor](./fluent-validation-executor.md)
- [Result Pattern](../../extensions/result.md)
