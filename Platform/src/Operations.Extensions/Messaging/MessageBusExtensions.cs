// Copyright (c) ABCDEG. All rights reserved.

using Operations.Extensions.Abstractions.Messaging;
using Wolverine;

namespace Operations.Extensions.Messaging;

public static class MessageBusExtensions
{
    public static Task<TCommandResult> InvokeCommandAsync<TCommandResult>(this IMessageBus bus, ICommand<TCommandResult> command,
        CancellationToken cancellationToken = default)
    {
        return bus.InvokeAsync<TCommandResult>(command, cancellationToken);
    }

    public static Task<TQueryResult> InvokeQueryAsync<TQueryResult>(this IMessageBus bus, IQuery<TQueryResult> query,
        CancellationToken cancellationToken = default)
    {
        return bus.InvokeAsync<TQueryResult>(query, cancellationToken);
    }
}
