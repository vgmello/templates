// Copyright (c) ABCDEG. All rights reserved.

using Billing.Contracts.Cashier.IntegrationEvents;
using CashierEntity = Billing.Cashier.Data.Entities.Cashier;
using CashierModel = Billing.Contracts.Cashier.Models.Cashier;
using Dapper;
using Npgsql;
using Operations.Extensions;
using Operations.Extensions.Messaging;
using FluentValidation;
using Wolverine;

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
    public record InsertCashierCommand(Guid CashierId, string Name, string? Email) : ICommand<int>;

    public static async Task<(Result<CashierModel>, CashierCreatedEvent)> Handle(
        CreateCashierCommand command, IMessageBus messaging, CancellationToken cancellationToken)
    {
        var cashierId = Guid.NewGuid();
        var insertCommand = new InsertCashierCommand(cashierId, command.Name, command.Email);
        
        var number = await messaging.InvokeCommandAsync(insertCommand, cancellationToken);

        var result = new CashierModel
        {
            CashierId = cashierId,
            CashierNumber = number,
            Name = command.Name,
            Email = command.Email ?? string.Empty
        };

        return (result, new CashierCreatedEvent(result));
    }

    public static async Task<int> Handle(InsertCashierCommand command, NpgsqlDataSource dataSource, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        
        // Call the stored function to create cashier
        await connection.ExecuteAsync(
            "SELECT billing.create_cashier(@cashier_id, @name, @email)",
            command);
        
        // Return the current count of cashiers as the cashier number
        var cashierNumber = await connection.QuerySingleAsync<int>(
            "SELECT COUNT(*) FROM billing.cashiers");
        
        return cashierNumber;
    }
}
