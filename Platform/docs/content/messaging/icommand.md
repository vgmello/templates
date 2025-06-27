---
title: ICommand<TResult> for command messaging
description: Learn about the ICommand<TResult> interface, a marker interface for defining commands that produce a result of a specific type.
---

# ICommand<TResult> for command messaging

The `ICommand<TResult>` interface serves as a marker interface within the messaging system. It signifies that a class represents a command that, upon execution, is expected to produce a result of type `TResult`.

This interface is primarily used for type identification and to enable generic handling of commands that return values, distinguishing them from commands that do not produce a direct return value (which might implement a different, non-generic command interface or simply be handled as a side effect).

## Understanding ICommand<TResult>

*   **Marker Interface**: `ICommand<TResult>` itself does not define any methods that you need to implement. Its purpose is to categorize and identify command objects that yield a result.
*   **`TResult`**: The `out` keyword on `TResult` indicates that the type parameter is covariant, meaning if `TResult` is a more derived type, `ICommand<TResult>` can be treated as `ICommand<BaseType>`. This is useful for flexibility in handling command results.
*   **`Empty` Property**: The `Empty` property provides a default value for `TResult`. This can be useful in scenarios where a command might not always produce a meaningful result, or for initialization purposes.

## Usage example

You typically implement `ICommand<TResult>` on a record or class that encapsulates the data required to perform a specific action and expects a result. The actual logic for handling and executing the command is usually separated into a command handler.

```csharp
using Operations.Extensions.Abstractions.Messaging;

// Define a command that creates a user and returns the new user's ID (int)
public record CreateUserCommand(string UserName, string Email) : ICommand<int>;

// Define a command that updates a product and returns a boolean indicating success
public record UpdateProductCommand(int ProductId, string ProductName) : ICommand<bool>;

// Define a command that processes an order and returns a complex OrderReceipt object
public record ProcessOrderCommand(Guid OrderId) : ICommand<OrderReceipt>;

public class OrderReceipt
{
    public Guid ReceiptId { get; set; }
    public decimal TotalAmount { get; set; }
}

// In your application, you would then have handlers that process these commands:
public class CreateUserCommandHandler // (Conceptual - not part of ICommand itself)
{
    public int Handle(CreateUserCommand command)
    {
        // Logic to create user in database
        Console.WriteLine($"Creating user: {command.UserName}");
        return 123; // Return new user ID
    }
}
```

## See also

*   [IQuery<TResult>](iquery.md)
