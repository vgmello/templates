// Copyright (c) ABCDEG. All rights reserved.

using Billing.Contracts.Cashier.IntegrationEvents;
using FluentValidation;
using System.Data;
using Wolverine;
using Wolverine.Persistence;
using CashierEntity = Billing.Cashier.Data.Entities.Cashier;

namespace Billing.Cashier.Commands;

public record CreateCashierCommand
{
    public string Name { get; set; } = null!;
}

public class CreateCustomerValidator : AbstractValidator<CreateCashierCommand>
{
    public CreateCustomerValidator()
    {
        RuleFor(c => c.Name).NotEmpty();
        RuleFor(c => c.Name).MaximumLength(100);
        RuleFor(c => c.Name).MinimumLength(2);
    }
}

public static class CreateCashierCommandHandler
{
    public static async Task<CashierCreatedEvent> Handle(CreateCashierCommand command, IMessageBus bus)
    {
        var entity = new CashierEntity
        {
            CashierId = Guid.NewGuid(),
            Name = command.Name,
            CreatedDateUtc = DateTime.UtcNow,
            UpdatedDateUtc = DateTime.UtcNow
        };

        var number = await bus.InvokeAsync<int>(new Insert<CashierEntity>(entity));

        var result = new Contracts.Cashier.Models.Cashier
        {
            CashierId = entity.CashierId,
            CashierNumber = number,
            Name = entity.Name
        };

        return new CashierCreatedEvent(result);
    }

    public static Task<int> Handle(Insert<CashierEntity> command, IDbConnection connection)
    {
        return Task.FromResult(1);
    }
}
