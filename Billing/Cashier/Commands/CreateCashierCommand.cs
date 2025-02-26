// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashier.Data.Persistence;
using Billing.Contracts.Cashier.IntegrationEvents;
using MassTransit;
using MediatR;

namespace Billing.Cashier.Commands;

public class CreateCashierCommand : IRequest
{
    public string Name { get; set; } = null!;
}

public class CreateCashierCommandHandler(IMediator mediator, IBus bus) : IRequestHandler<CreateCashierCommand>
{
    public async Task Handle(CreateCashierCommand request, CancellationToken cancellationToken)
    {
        var cashier = new Data.Entities.Cashier
        {
            Name = request.Name
        };

        await mediator.Send(new AddCashierDbCommand { Cashier = cashier }, cancellationToken);
        await bus.Publish(new CashierCreated() { Cashier = cashier }, cancellationToken);
    }
}
