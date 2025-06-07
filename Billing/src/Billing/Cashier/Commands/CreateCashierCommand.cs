// Copyright (c) ABCDEG. All rights reserved.

using Billing.Contracts.Cashier.IntegrationEvents;
using Dapper;
using Npgsql;
using System.Data;
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
    public record InsertCashierCommand(Guid CashierId, string Name, string Email) : ICommand<int>;

    public static async Task<(Result<CashierModel>, CashierCreatedEvent)> Handle(
        CreateCashierCommand command, IMessageContext messaging, CancellationToken cancellationToken)
    {
        var insert = new InsertCashierCommand(Guid.NewGuid(), command.Name, command.Email);

        var number = await messaging.InvokeCommandAsync(insert, cancellationToken);

        var result = new CashierModel
        {
            CashierId = insert.CashierId,
            CashierNumber = number,
            Name = insert.Name
        };

        return (result, new CashierCreatedEvent(result));
    }

    public static async Task<int> Handle(
        InsertCashierCommand command,
        NpgsqlDataSource dataSource,
        CancellationToken cancellationToken)
    {
        using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var parameters = new { cashier_id = command.CashierId, name = command.Name, email = command.Email };

        var number = await connection.ExecuteScalarAsync<int>(
            "billing.create_cashier",
            parameters,
            commandType: CommandType.StoredProcedure);

        return number;
    }
}
