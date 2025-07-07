// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashiers.Contracts.IntegrationEvents;
using Billing.Cashiers.Contracts.Models;

namespace Billing.Cashiers.Commands;

using CashierModel = Cashier;

public record CreateCashierCommand(Guid TenantId, string Name, string Email) : ICommand<Result<CashierModel>>;

public class CreateCashierValidator : AbstractValidator<CreateCashierCommand>
{
    public CreateCashierValidator()
    {
        RuleFor(c => c.Name).NotEmpty();
        RuleFor(c => c.Name).MaximumLength(100);
        RuleFor(c => c.Name).MinimumLength(2);
    }
}

public static partial class CreateCashierCommandHandler
{
    [DbCommand(sp: "billing.cashiers_create", nonQuery: true)]
    public partial record InsertCashierCommand(Guid CashierId, string Name, string? Email) : ICommand<int>;

    public static async Task<(Result<CashierModel>, CashierCreated?)> Handle(CreateCashierCommand command, IMessageBus messaging,
        CancellationToken cancellationToken)
    {
        if (command.Name.Contains("error"))
        {
            throw new DivideByZeroException("Forced test unhandled exception to simulate error scenarios");
        }

        var cashierId = Guid.CreateVersion7();

        var insertCommand = new InsertCashierCommand(cashierId, command.Name, command.Email);

        await messaging.InvokeCommandAsync(insertCommand, cancellationToken);

        var result = new CashierModel
        {
            TenantId = command.TenantId,
            CashierId = cashierId,
            Name = command.Name,
            Email = command.Email
        };

        var createdEvent = new CashierCreated(result.TenantId, PartitionKeyTest: 0, result);

        return (result, createdEvent);
    }
}
