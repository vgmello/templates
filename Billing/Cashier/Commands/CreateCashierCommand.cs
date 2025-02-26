// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashier.Data.Persistence;
using Billing.Contracts.Cashier.IntegrationEvents;
using MediatR;
using Operations.ServiceDefaults.Infrastructure.Mediator;

namespace Billing.Cashier.Commands;

public class CreateCashierCommand : IRequest<Guid>
{
    public string Name { get; set; } = null!;
}

public class CreateCashierCommandHandler(ICommandServices services) : CommandHandler<CreateCashierCommand, Guid>(services)
{
    protected override async Task<Guid> Handle(CreateCashierCommand request)
    {
        var cashier = new Data.Entities.Cashier
        {
            Name = request.Name
        };

        var cashierId = await SendCommand(new AddCashierDbCommand(cashier));

        await PublishEvent(new CashierCreatedEvent(cashierId));

        return cashierId;
    }
}
