// Copyright (c) ABCDEG. All rights reserved.

using Wolverine;

namespace Operations.Extensions.Messaging;

public static class MessageBusExtensions
{
    public static Task<TCommandResult> InvokeCommandAsync<TCommandResult>(this IMessageBus bus,
        ICommand<TCommandResult> command, CancellationToken cancellationToken = default)
    {
        return bus.InvokeAsync<TCommandResult>(command, cancellationToken);
    }
}
