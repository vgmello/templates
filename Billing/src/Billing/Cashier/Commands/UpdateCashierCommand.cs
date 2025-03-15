// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashier.Data.Persistence;
using Billing.Contracts.Cashier.IntegrationEvents;

namespace Billing.Cashier.Commands;

public record UpdateCashierCommand : IRequest<Guid>
{
    public string Name { get; set; } = null!;
}

public class UpdateCashierCommandHandler(ICommandServices services)
    : CommandHandler<UpdateCashierCommand, Guid>(services)
{
    protected override async Task<Guid> Handle(UpdateCashierCommand command)
    {
        var cashier = new Data.Entities.Cashier
        {
            Name = command.Name
        };

        var cashierId = await SendCommand(new AddCashierDbCommand(cashier));

        await PublishEvent(new CashierUpdatedEvent(cashierId));

        return cashierId;
    }
}
