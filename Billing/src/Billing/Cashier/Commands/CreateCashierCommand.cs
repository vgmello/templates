// Copyright (c) ABCDEG. All rights reserved.

using System; // For Guid
using System.Threading;
using System.Threading.Tasks;
using Billing.Contracts.Cashier.IntegrationEvents;
using Billing.Contracts.Cashier.Models;
using Operations.Extensions; // For Result
using Operations.Extensions.Messaging; // For ICommand, IMessageBus
using FluentValidation;
// Npgsql is no longer directly used by this handler.
// using Npgsql;

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

public static partial class CreateCashierCommandHandler
{
    public static async Task<(Result<CashierModel>, CashierCreatedEvent)> Handle(
        CreateCashierCommand command,
        IMessageBus messaging, // Changed from NpgsqlDataSource to IMessageBus
        CancellationToken cancellationToken)
    {
        var cashierId = Guid.NewGuid();

        var insertCommand = new InsertCashierCommand(cashierId, command.Name, command.Email);

        // Reverted to using IMessageBus to invoke the command.
        // The IMessageBus infrastructure is expected to find and execute the
        // source-generated InsertCashierCommandHandler.HandleAsync.
        // If InsertCashierCommand returns int (rows affected), InvokeCommandAsync<int> should be used.
        // The previous feedback used `await messaging.InvokeCommandAsync(insertCommand, cancellationToken);`
        // which implies either the bus handles the type dispatch or it's a fire-and-forget.
        // Given ICommand<int>, we expect an int result.

        int rowsAffected = await messaging.InvokeCommandAsync(insertCommand, cancellationToken);

        // It's important that the message bus infrastructure and the source-generated handler for ICommand<int>
        // correctly return the integer result from the database operation.
        // The `NonQuery = true` on `InsertCashierCommand` means the generated handler will use ExecuteAsync,
        // which returns rows affected - this is consistent with `ICommand<int>`.

        if (rowsAffected == 0)
        {
            // Handle case where cashier wasn't created (no rows affected).
            var failureModel = new CashierModel { CashierId = cashierId, Name = command.Name, Email = command.Email };
            return (Result.Fail<CashierModel>("Failed to create cashier. The database operation reported no rows affected."),
                    new CashierCreatedEvent(failureModel, "CreationFailed:NoRowsAffected"));
        }

        var result = new CashierModel
        {
            CashierId = cashierId,
            Name = command.Name,
            Email = command.Email
        };

        var createdEvent = new CashierCreatedEvent(result);
        return (Result.Ok(result), createdEvent);
    }
}
