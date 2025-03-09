// Copyright (c) ABCDEG. All rights reserved.

using MassTransit;
using MediatR;

namespace Operations.ServiceDefaults.Mediator;

public abstract class CommandHandler<TCommand, TCommandResult>(ICommandServices services) : IRequestHandler<TCommand, TCommandResult>
    where TCommand : IRequest<TCommandResult>
{
    protected CancellationToken CancellationToken { get; private set; }

    protected IMediator Mediator => services.Mediator;

    public Task<TCommandResult> Handle(TCommand request, CancellationToken cancellationToken)
    {
        CancellationToken = cancellationToken;

        return Handle(request);
    }

    protected abstract Task<TCommandResult> Handle(TCommand command);

    protected Task<TResult> SendCommand<TResult>(IRequest<TResult> command) => Mediator.Send(command, CancellationToken);

    protected Task SendCommand(IRequest command) => Mediator.Send(command, CancellationToken);

    protected Task<TResult> SendQuery<TResult>(IRequest<TResult> query) => Mediator.Send(query, CancellationToken);

    /// <summary>
    ///     Publishes an event to the event bus
    /// </summary>
    /// <param name="event">Event</param>
    /// <typeparam name="TEvent">Event Type</typeparam>
    protected Task PublishEvent<TEvent>(TEvent @event) where TEvent : notnull => Mediator.Publish(@event, CancellationToken);
}
