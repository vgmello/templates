// Copyright (c) ABCDEG. All rights reserved.

using Billing.Contracts.Cashier.IntegrationEvents;
using CashierEntity = Billing.Cashier.Data.Entities.Cashier;
using CashierModel = Billing.Contracts.Cashier.Models.Cashier;

namespace Billing.Cashier.Commands;

public record CreateCashierCommand(string Name, string Email) : ICommand<Result<CashierModel>>;

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

    public static async Task<(Result<CashierModel>, CashierCreatedEvent)> Handle(
        CreateCashierCommand command, IMessageContext messaging, CancellationToken cancellationToken)
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
            Name = entity.Name,
            Email = entity.Email
        };

        return (result, new CashierCreatedEvent(result));
    }

    public static Task<int> Handle(InsertCashierCommand command, CancellationToken cancellationToken)
    {
        return Task.FromResult(command.Cashier.CashierNumber + 1);
    }
}
