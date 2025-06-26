// Copyright (c) ABCDEG. All rights reserved.

using Billing.Contracts.Cashier.IntegrationEvents;
using FluentValidation.Results;

namespace Billing.Cashier.Commands;

public record DeleteCashierCommand(Guid CashierId) : ICommand<Result<bool>>;

public class DeleteCashierValidator : AbstractValidator<DeleteCashierCommand>
{
    public DeleteCashierValidator()
    {
        RuleFor(c => c.CashierId).NotEmpty();
    }
}

public static partial class DeleteCashierCommandHandler
{
    [DbCommand(sp: "billing.cashier_delete", nonQuery: true)]
    public partial record DeleteCashierDbCommand(Guid CashierId) : ICommand<int>;

    public static async Task<(Result<bool>, CashierDeleted?)> Handle(DeleteCashierCommand command, IMessageBus messaging,
        CancellationToken cancellationToken)
    {
        var deleteDbCommand = new DeleteCashierDbCommand(command.CashierId);

        var rowsAffected = await messaging.InvokeCommandAsync(deleteDbCommand, cancellationToken);

        if (rowsAffected == 0)
        {
            var failures = new List<ValidationFailure> { new("CashierId", "Cashier not found") };

            return (failures, null);
        }

        var deletedEvent = new CashierDeleted(command.CashierId);

        return (true, deletedEvent);
    }
}
