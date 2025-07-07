---
title: Result type for operation outcomes
description: Learn how to use the Result<T> type to represent the outcome of an operation, encapsulating either a successful result or a list of validation failures.
---

# Result type for operation outcomes

The `Result<T>` type provides a robust way to represent the outcome of an operation that can either succeed with a value of type `T` or fail with a collection of validation errors. This pattern helps you write more predictable and error-resilient code by explicitly handling both success and failure scenarios.

## Understanding Result<T>

`Result<T>` is built upon the `OneOf` library, allowing it to hold one of two possible states:

*   **Success**: Contains an instance of `T`, representing the successful outcome of the operation.
*   **Failure**: Contains a `List<ValidationFailure>`, providing detailed information about any validation errors that occurred.

This design encourages you to handle both successful results and potential errors at the point of consumption, leading to clearer control flow and fewer unexpected runtime exceptions.

## Usage example

You can use `Result<T>` in your methods to return either the expected data or a list of validation errors. Here's how you might define a service method and consume its result:

```csharp
// Example: Defining a service method that returns Result<T>
public class UserService
{
    public Result<User> CreateUser(User user)
    {
        var validationFailures = new List<ValidationFailure>();

        if (string.IsNullOrWhiteSpace(user.Name))
        {
            validationFailures.Add(new ValidationFailure("Name", "User name cannot be empty."));
        }

        if (user.Age < 18)
        {
            validationFailures.Add(new ValidationFailure("Age", "User must be at least 18 years old."));
        }

        if (validationFailures.Any())
        {
            return new Result<User>(validationFailures);
        }

        // Simulate successful user creation
        Console.WriteLine($"User '{user.Name}' created successfully.");
        return new Result<User>(user);
    }
}

// Example: Consuming the Result<T>
public class User
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public class Program
{
    public static void Main(string[] args)
    {
        var userService = new UserService();

        // Successful scenario
        var validUser = new User { Name = "Alice", Age = 30 };
        Result<User> successResult = userService.CreateUser(validUser);

        successResult.Switch(
            user => Console.WriteLine($"Successfully created user: {user.Name}"),
            failures =>
            {
                Console.WriteLine("Failed to create user. Validation errors:");
                foreach (var failure in failures)
                {
                    Console.WriteLine($" - {failure.PropertyName}: {failure.ErrorMessage}");
                }
            });

        Console.WriteLine();

        // Failure scenario
        var invalidUser = new User { Name = "", Age = 16 };
        Result<User> failureResult = userService.CreateUser(invalidUser);

        failureResult.Switch(
            user => Console.WriteLine($"Successfully created user: {user.Name}"),
            failures =>
            {
                Console.WriteLine("Failed to create user. Validation errors:");
                foreach (var failure in failures)
                {
                    Console.WriteLine($" - {failure.PropertyName}: {failure.ErrorMessage}");
                }
            });
    }
}
```

## See also

*   [OneOf Library](https://github.com/mcintyre321/OneOf)
*   [FluentValidation Library](https://fluentvalidation.net/)
