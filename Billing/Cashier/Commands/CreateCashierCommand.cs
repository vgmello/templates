// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashier.Data.Persistence;
using Billing.Contracts.Cashier.IntegrationEvents;
using MassTransit;
using MediatR;

namespace Billing.Cashier.Commands;

public class CreateCashierCommand : IRequest<Guid>
{
    public string Name { get; set; } = null!;
}

public class CreateCashierCommandHandler(IMediator mediator, IBus bus) : IRequestHandler<CreateCashierCommand, Guid>
{
    public async Task<Guid> Handle(CreateCashierCommand request, CancellationToken cancellationToken)
    {
        var cashier = new Data.Entities.Cashier
        {
            Name = request.Name
        };

        var

        var cashierId = await mediator.Send(new AddCashierDbCommand(cashier), cancellationToken);




        await bus.Publish(new CashierCreatedEvent(cashierId), cancellationToken);

        return cashierId;
    }
}
