---
title: FluentValidation Executor
description: Discover the FluentValidationExecutor, a utility for executing single or multiple FluentValidation validators against messages.
---

# FluentValidation Executor

The `FluentValidationExecutor` is a utility class designed to simplify the execution of FluentValidation validators against messages within your application. It provides static methods to validate a message using either a single validator or a collection of validators, returning a list of validation failures.

## Purpose

This executor streamlines the validation process, especially in messaging pipelines where messages need to be validated before further processing. It abstracts away the direct interaction with `IValidator<T>` and `ValidateAsync`, providing a clean interface to get validation results.

## Key Methods

### ExecuteOne

`ExecuteOne` validates a single message against a single `IValidator<T>`. It's useful when you know exactly which validator applies to a given message type.

```csharp
public static async Task<List<ValidationFailure>> ExecuteOne<T>(IValidator<T> validator, T message)
```

-   **`validator`**: The `IValidator<T>` instance to use for validation.
-   **`message`**: The message object to validate.
-   **Returns**: A `List<ValidationFailure>` containing any validation errors.

### ExecuteMany

`ExecuteMany` validates a single message against an enumerable collection of `IValidator<T>` instances. This is particularly useful when multiple validators might apply to the same message type, and you want to collect all validation failures from all relevant validators.

```csharp
public static async Task<List<ValidationFailure>> ExecuteMany<T>(IEnumerable<IValidator<T>> validators, T message)
```

-   **`validators`**: A collection of `IValidator<T>` instances to use for validation.
-   **`message`**: The message object to validate.
-   **Returns**: A `List<ValidationFailure>` containing all validation errors from all executed validators.

## Usage example

You would typically use `FluentValidationExecutor` within a Wolverine middleware or a message handler where you need to perform validation.

```csharp
using FluentValidation;
using FluentValidation.Results;
using Operations.ServiceDefaults.Messaging.Middlewares;

public class MyMessage
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public class MyMessageValidator : AbstractValidator<MyMessage>
{
    public MyMessageValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Age).GreaterThan(0);
    }
}

public class MyMessageHandler
{
    private readonly IValidator<MyMessage> _singleValidator;
    private readonly IEnumerable<IValidator<MyMessage>> _manyValidators;

    public MyMessageHandler(IValidator<MyMessage> singleValidator, IEnumerable<IValidator<MyMessage>> manyValidators)
    {
        _singleValidator = singleValidator;
        _manyValidators = manyValidators;
    }

    public async Task Handle(MyMessage message)
    {
        // Using ExecuteOne
        var singleValidationErrors = await FluentValidationExecutor.ExecuteOne(_singleValidator, message);
        if (singleValidationErrors.Any())
        {
            Console.WriteLine("Single validator errors:");
            foreach (var error in singleValidationErrors)
            {
                Console.WriteLine($" - {error.PropertyName}: {error.ErrorMessage}");
            }
        }

        // Using ExecuteMany
        var manyValidationErrors = await FluentValidationExecutor.ExecuteMany(_manyValidators, message);
        if (manyValidationErrors.Any())
        {
            Console.WriteLine("Many validators errors:");
            foreach (var error in manyValidationErrors)
            {
                Console.WriteLine($" - {error.PropertyName}: {error.ErrorMessage}");
            }
        }

        // ... rest of message handling logic
    }
}
```

## See also

- [Fluent Validation Policy](./fluent-validation-policy.md)
