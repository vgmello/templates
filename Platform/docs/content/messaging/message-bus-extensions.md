---
title: MessageBus extensions for command and query invocation
description: Learn about the MessageBus extension methods that simplify invoking commands and queries using Wolverine.
---

# MessageBus extensions for command and query invocation

The `MessageBusExtensions` class provides convenient extension methods for `IMessageBus` objects, specifically designed to simplify the invocation of commands and queries within your application. These extensions leverage the Wolverine messaging library to dispatch and handle your `ICommand<TResult>` and `IQuery<TResult>` messages.

## Understanding MessageBusExtensions

This class offers the following key methods:

*   **`InvokeCommandAsync<TCommandResult>(this IMessageBus bus, ICommand<TCommandResult> command, CancellationToken cancellationToken = default)`**: This method allows you to asynchronously invoke a command that is expected to return a result of type `TCommandResult`. It takes an `IMessageBus` instance, the command object, and an optional `CancellationToken`.

*   **`InvokeQueryAsync<TQueryResult>(this IMessageBus bus, IQuery<TQueryResult> query, CancellationToken cancellationToken = default)`**: Similar to `InvokeCommandAsync`, this method asynchronously invokes a query that is expected to return a result of type `TQueryResult`. It also takes an `IMessageBus` instance, the query object, and an optional `CancellationToken`.

These extensions provide a clean and type-safe way to interact with your messaging infrastructure, abstracting away the underlying details of message dispatch.

## Usage examples

To use these extensions, you typically inject `IMessageBus` into your services or controllers. Then, you can simply call `InvokeCommandAsync` or `InvokeQueryAsync` with your command or query objects.

```csharp
using Operations.Extensions.Abstractions.Messaging;
using Operations.Extensions.Messaging;
using Wolverine;
using System.Threading.Tasks;
using System.Threading;
using System;

// Assume IMessageBus is registered in your DI container
// public class MyService(IMessageBus messageBus)
// {
//     private readonly IMessageBus _messageBus = messageBus;
// }

// Define a command and a query (as shown in ICommand and IQuery documentation)
public record CreateUserCommand(string UserName, string Email) : ICommand<int>;
public record GetUserByIdQuery(int UserId) : IQuery<User>;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// Example of a service using the MessageBus extensions
public class ApplicationService
{
    private readonly IMessageBus _messageBus;

    public ApplicationService(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    public async Task<int> CreateNewUser(string userName, string email)
    {
        var command = new CreateUserCommand(userName, email);
        // Highlight the usage of InvokeCommandAsync
        [!code-csharp[InvokeCommandExample](~/samples/messaging/message-bus-extensions-sample.cs?name=InvokeCommandExample)]
        return userId;
    }

    public async Task<User> GetUserDetails(int userId)
    {
        var query = new GetUserByIdQuery(userId);
        // Highlight the usage of InvokeQueryAsync
        [!code-csharp[InvokeQueryExample](~/samples/messaging/message-bus-extensions-sample.cs?name=InvokeQueryExample)]
        return user;
    }
}

// Sample code for the snippets (docs/samples/messaging/message-bus-extensions-sample.cs)
// This file would contain:
/*
namespace SampleApp
{
    using Operations.Extensions.Abstractions.Messaging;
    using Operations.Extensions.Messaging;
    using Wolverine;
    using System.Threading.Tasks;
    using System.Threading;

    public partial class ApplicationService
    {
        // <InvokeCommandExample>
        public async Task<int> CreateUser(string userName, string email)
        {
            var command = new CreateUserCommand(userName, email);
            var userId = await _messageBus.InvokeCommandAsync(command);
            return userId;
        }
        // </InvokeCommandExample>

        // <InvokeQueryExample>
        public async Task<User> GetUser(int userId)
        {
            var query = new GetUserByIdQuery(userId);
            var user = await _messageBus.InvokeQueryAsync(query);
            return user;
        }
        // </InvokeQueryExample>
    }
}
*/
```

## See also

*   [Wolverine](https://wolverine.netlify.app/)
*   [ICommand<TResult>](icommand.md)
*   [IQuery<TResult>](iquery.md)
