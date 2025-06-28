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

To use these extensions, you typically inject `IMessageBus` into your services or controllers. Then, call `InvokeCommandAsync` or `InvokeQueryAsync` with your command or query objects.

[!code-csharp[InvokeCommandExample](~/samples/messaging/message-bus-extensions-sample.cs?name=InvokeCommandExample&highlight=4)]

[!code-csharp[InvokeQueryExample](~/samples/messaging/message-bus-extensions-sample.cs?name=InvokeQueryExample&highlight=4)]

## See also

*   [Wolverine](https://wolverine.netlify.app/)
*   [ICommand<TResult>](icommand.md)
*   [IQuery<TResult>](iquery.md)
