// Copyright (c) ABCDEG. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using Billing.Contracts.Cashier.IntegrationEvents;
using Billing.Contracts.Cashier.Models;
using Operations.Extensions;
using Operations.Extensions.Messaging;
using Operations.Extensions.Dapper; // Added for DbCommandAttribute
using FluentValidation;

namespace Billing.Cashier.Commands;

// CreateCashierCommand is defined in the same namespace as CreateCashierCommandHandler
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
    // Nested InsertCashierCommand definition:
    [DbCommand(sp: "billing.create_cashier", UseSnakeCase = true, ReturnsAffectedRecords = true, NonQuery = true)]
    public partial record InsertCashierCommand(Guid CashierId, string Name, string? Email) : ICommand<int>;

    public static async Task<(Result<CashierModel>, CashierCreatedEvent)> Handle(
        CreateCashierCommand command,
        IMessageBus messaging,
        CancellationToken cancellationToken)
    {
        var cashierId = Guid.NewGuid();

        // This now refers to the nested CreateCashierCommandHandler.InsertCashierCommand
        var insertCommand = new InsertCashierCommand(cashierId, command.Name, command.Email);

        await messaging.InvokeCommandAsync(insertCommand, cancellationToken);

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
