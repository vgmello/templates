// Copyright (c) ABCDEG. All rights reserved.

using MediatR;

namespace Operations.ServiceDefaults.Mediator;

public abstract class QueryHandler<TQuery, TTQueryResult>(IQueryServices services) : IRequestHandler<TQuery, TTQueryResult>
    where TQuery : IRequest<TTQueryResult>
{
    protected CancellationToken CancellationToken { get; private set; }

    protected IMediator Mediator => services.Mediator;

    public Task<TTQueryResult> Handle(TQuery request, CancellationToken cancellationToken)
    {
        CancellationToken = cancellationToken;

        return Handle(request);
    }

    protected abstract Task<TTQueryResult> Handle(TQuery query);

    protected Task<TResult> SendQuery<TResult>(IRequest<TResult> query) => Mediator.Send(query, CancellationToken);
}
