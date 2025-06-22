// Copyright (c) ABCDEG. All rights reserved.

using Billing.Contracts.Cashier.IntegrationEvents;
using FluentValidation.Results;

namespace Billing.Cashier.Commands;

using CashierModel = Billing.Contracts.Cashier.Models.Cashier;

public record UpdateCashierCommand(Guid CashierId, string Name, string? Email) : ICommand<Result<CashierModel>>;

public class UpdateCashierValidator : AbstractValidator<UpdateCashierCommand>
{
    public UpdateCashierValidator()
    {
        RuleFor(c => c.CashierId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty();
        RuleFor(c => c.Name).MaximumLength(100);
        RuleFor(c => c.Name).MinimumLength(2);
    }
}

public static partial class UpdateCashierCommandHandler
{
    [DbCommand(sp: "billing.update_cashier", nonQuery: true)]
    public partial record UpdateCashierDbCommand(Guid CashierId, string Name, string? Email) : ICommand<int>;

    public static async Task<(Result<CashierModel>, CashierUpdatedEvent?)> Handle(UpdateCashierCommand command, IMessageBus messaging,
        CancellationToken cancellationToken)
    {
        var updateDbCommand = new UpdateCashierDbCommand(command.CashierId, command.Name, command.Email);

        var rowsAffected = await messaging.InvokeCommandAsync(updateDbCommand, cancellationToken);

        if (rowsAffected == 0)
        {
            var failures = new List<ValidationFailure> { new("CashierId", "Cashier not found") };

            return (failures, null);
        }

        var result = new CashierModel
        {
            CashierId = command.CashierId,
            Name = command.Name,
            Email = command.Email ?? "Not Updated"
        };

        var updatedEvent = new CashierUpdatedEvent(command.CashierId);

        return (result, updatedEvent);
    }
}
