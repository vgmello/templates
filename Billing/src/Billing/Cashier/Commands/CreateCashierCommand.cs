// Copyright (c) ABCDEG. All rights reserved.

using Billing.Contracts.Cashier.IntegrationEvents;
using CashierModel = Billing.Contracts.Cashier.Models.Cashier;
using Operations.Extensions.Dapper;
using Npgsql;

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
    [DbParams]
    public partial record InsertCashierCommand(Guid CashierId, string Name, string? Email) : ICommand<int>;

    public static async Task<(Result<CashierModel>, CashierCreatedEvent)> Handle(
        CreateCashierCommand command, IMessageBus messaging, CancellationToken cancellationToken)
    {
        var cashierId = Guid.NewGuid();

        var insertCommand = new InsertCashierCommand(cashierId, command.Name, command.Email);

        await messaging.InvokeCommandAsync(insertCommand, cancellationToken);

        var result = new CashierModel
        {
            CashierId = cashierId,
            Name = command.Name,
            Email = command.Email
        };

        return (result, new CashierCreatedEvent(result));
    }

    public static async Task<int> Handle(InsertCashierCommand command, NpgsqlDataSource dataSource, CancellationToken cancellationToken)
    {
        return await dataSource.CallSp("billing.create_cashier", command.ToDbParams(), cancellationToken);
    }
}
