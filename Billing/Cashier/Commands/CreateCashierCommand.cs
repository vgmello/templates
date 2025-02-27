// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashier.Data.Persistence;
using Billing.Contracts.Cashier.IntegrationEvents;

namespace Billing.Cashier.Commands;

public record CreateCashierCommand : IRequest<Contracts.Cashier.Models.Cashier>
{
    public string Name { get; set; } = null!;
}

public class CreateCashierCommandHandler(ICommandServices services) : CommandHandler<CreateCashierCommand, Contracts.Cashier.Models.Cashier>(services)
{
    protected override async Task<Contracts.Cashier.Models.Cashier> Handle(CreateCashierCommand command)
    {
        var entity = new Data.Entities.Cashier
        {
            Name = command.Name
        };

        var cashierId = await SendCommand(new AddCashierDbCommand(entity));

        await PublishEvent(new CashierCreatedEvent(cashierId));

        var cashier = await SendQuery(new GetCashierByIdDbQuery(cashierId));

        return new Contracts.Cashier.Models.Cashier
        {
            CashierId = cashier.CashierId,
            Name = cashier.Name
        };
    }
}
