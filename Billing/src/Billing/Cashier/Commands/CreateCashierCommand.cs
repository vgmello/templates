// Copyright (c) ABCDEG. All rights reserved.

using Billing.Contracts.Cashier.IntegrationEvents;
using FluentValidation;
using Operations.Extensions.Messaging;
using System.Data;
using Wolverine;
using CashierEntity = Billing.Cashier.Data.Entities.Cashier;
using CashierModel = Billing.Contracts.Cashier.Models.Cashier;

namespace Billing.Cashier.Commands;

public record CreateCashierCommand(string Name, string Email) : ICommand<CashierModel>;

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
    public record InsertCashierCommand(CashierEntity Cashier) : ICommand<int>;

    public static async Task<(CashierModel, CashierCreatedEvent)> Handle(CreateCashierCommand command,
        IMessageContext messaging,
        CancellationToken cancellationToken)
    {
        var entity = new CashierEntity
        {
            CashierId = Guid.NewGuid(),
            Name = command.Name,
            Email = command.Email
        };

        var number = await messaging.InvokeCommandAsync(new InsertCashierCommand(entity), cancellationToken);

        var result = new CashierModel
        {
            CashierId = entity.CashierId,
            CashierNumber = number,
            Name = entity.Name
        };

        return (result, new CashierCreatedEvent(result));
    }

    public static Task<int> Handle(InsertCashierCommand command, IDbConnection connection,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(1);
    }
}
