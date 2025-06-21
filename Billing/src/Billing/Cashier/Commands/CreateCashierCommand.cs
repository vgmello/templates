// Copyright (c) ABCDEG. All rights reserved.

using Billing.Contracts.Cashier.IntegrationEvents;

namespace Billing.Cashier.Commands;

using CashierModel = Billing.Contracts.Cashier.Models.Cashier;

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

public static partial class CreateCashierCommandHandler
{
    [DbCommand(sp: "billing.create_cashier", nonQuery: true)]
    public partial record InsertCashierCommand(Guid CashierId, string Name, string? Email) : ICommand<int>;

    public static async Task<(Result<CashierModel>, CashierCreatedEvent)> Handle(CreateCashierCommand command, IMessageBus messaging,
        CancellationToken cancellationToken)
    {
        if (command.Name.Contains("error"))
        {
            throw new DivideByZeroException("Forced test exception to simulate error scenarios");
        }

        var cashierId = Guid.CreateVersion7();

        var insertCommand = new InsertCashierCommand(cashierId, command.Name, command.Email);

        await messaging.InvokeCommandAsync(insertCommand, cancellationToken);

        var result = new CashierModel
        {
            CashierId = cashierId,
            Name = command.Name,
            Email = command.Email
        };

        var createdEvent = new CashierCreatedEvent(result);

        return (result, createdEvent);
    }
}
