// Copyright (c) ABCDEG. All rights reserved.

using System; // For Guid
using System.Threading;
using System.Threading.Tasks;
using Billing.Contracts.Cashier.IntegrationEvents;
using Billing.Contracts.Cashier.Models; // For CashierModel (assuming namespace)
using Operations.Extensions; // For Result
using Operations.Extensions.Messaging; // For ICommand (on CreateCashierCommand)
using FluentValidation;
using Npgsql; // For NpgsqlDataSource
// Using for the new, separate InsertCashierCommand is implicit if in same namespace,
// but good to be mindful if it were different. It's in Billing.Cashier.Commands.

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
    // The InternalInsertCashierDbCommand record is now removed from here.
    // It's replaced by the separate InsertCashierCommand file and its generated handler.

    public static async Task<(Result<CashierModel>, CashierCreatedEvent)> Handle(
        CreateCashierCommand command,
        NpgsqlDataSource dataSource,
        CancellationToken cancellationToken)
    {
        var cashierId = Guid.NewGuid();

        // Create the new, separate InsertCashierCommand
        var insertCommand = new InsertCashierCommand(cashierId, command.Name, command.Email);

        // Call the source-generated handler for InsertCashierCommand.
        // The DbCommandSourceGenerator is expected to create an InsertCashierCommandHandler class
        // with a static HandleAsync method.
        int rowsAffected = await InsertCashierCommandHandler.HandleAsync(insertCommand, dataSource, cancellationToken);

        if (rowsAffected == 0)
        {
            // Handle case where cashier wasn't created, e.g., return a failure result.
            // This specific error handling (message, event details) can be tuned.
            // For now, creating a simple failure result.
            var failureModel = new CashierModel { CashierId = cashierId, Name = command.Name, Email = command.Email };
            return (Result.Fail<CashierModel>("Failed to create cashier in the database. No rows were affected."),
                    new CashierCreatedEvent(failureModel, "CreationFailed:DatabaseOperationFailed"));
        }

        var cashierModel = new CashierModel
        {
            CashierId = cashierId,
            Name = command.Name,
            Email = command.Email
        };

        var createdEvent = new CashierCreatedEvent(cashierModel);

        return (Result.Ok(cashierModel), createdEvent);
    }

    // Any manually written handler for a previous version of InsertCashierCommand
    // (like the one that used NpgsqlDataSourceExtensions.CallSp or ExecuteDbCommandAsync directly)
    // should have already been removed or is implicitly removed by this overwrite if it was here.
    // The key is that the call above to InsertCashierCommandHandler.HandleAsync relies on generated code.
}
