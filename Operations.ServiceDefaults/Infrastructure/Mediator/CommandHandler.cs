// Copyright (c) ABCDEG. All rights reserved.

using MassTransit;
using MediatR;

namespace Operations.ServiceDefaults.Infrastructure.Mediator;

public abstract class CommandHandler<TCommand, TCommandResult>(ICommandServices services) : IRequestHandler<TCommand, TCommandResult>
    where TCommand : IRequest<TCommandResult>
{
    protected CancellationToken CancellationToken { get; private set; }

    protected IMediator Mediator => services.Mediator;

    protected IBus EventBus => services.EventBus;

    public Task<TCommandResult> Handle(TCommand request, CancellationToken cancellationToken)
    {
        CancellationToken = cancellationToken;

        return Handle(request);
    }

    protected abstract Task<TCommandResult> Handle(TCommand request);

    protected Task<TResult> SendCommand<TResult>(IRequest<TResult> request) => Mediator.Send(request, CancellationToken);

    protected Task SendCommand(IRequest request) => Mediator.Send(request, CancellationToken);

    protected Task<TResult> SendQuery<TResult>(IRequest<TResult> request) => Mediator.Send(request, CancellationToken);

    /// <summary>
    ///     Publishes an event to the event bus
    /// </summary>
    /// <param name="event">Event</param>
    /// <typeparam name="TEvent">Event Type</typeparam>
    protected Task PublishEvent<TEvent>(TEvent @event) where TEvent : notnull => EventBus.Publish(@event, CancellationToken);
}
