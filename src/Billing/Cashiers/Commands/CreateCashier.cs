// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashiers.Contracts.IntegrationEvents;
using Billing.Cashiers.Contracts.Models;
using Billing.Cashiers.Data;
using Billing.Core.Data;
using LinqToDB;

namespace Billing.Cashiers.Commands;

public record CreateCashierCommand(Guid TenantId, string Name, string Email) : ICommand<Result<Cashier>>;

public class CreateCashierValidator : AbstractValidator<CreateCashierCommand>
{
    public CreateCashierValidator()
    {
        RuleFor(c => c.TenantId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty();
        RuleFor(c => c.Name).MinimumLength(2);
        RuleFor(c => c.Name).MaximumLength(100);
    }
}

public static class CreateCashierCommandHandler
{
    public record DbCommand(Data.Entities.Cashier Cashier) : ICommand<Data.Entities.Cashier>;

    public static async Task<(Result<Cashier>, CashierCreated?)> Handle(CreateCashierCommand command, IMessageBus messaging,
        CancellationToken cancellationToken)
    {
        var dbCommand = CreateInsertCommand(command);
        var insertedCashier = await messaging.InvokeCommandAsync(dbCommand, cancellationToken);

        var result = insertedCashier.ToModel();
        var createdEvent = new CashierCreated(result.TenantId, PartitionKeyTest: 0, result);

        return (result, createdEvent);
    }

    public static async Task<Data.Entities.Cashier> Handle(DbCommand command, BillingDb db, CancellationToken cancellationToken)
    {
        var inserted = await db.Cashiers.InsertWithOutputAsync(command.Cashier, token: cancellationToken);

        return inserted;
    }

    private static DbCommand CreateInsertCommand(CreateCashierCommand command) =>
        new(new Data.Entities.Cashier
        {
            TenantId = command.TenantId,
            CashierId = Guid.CreateVersion7(),
            Name = command.Name,
            Email = command.Email,
            CreatedDateUtc = DateTime.UtcNow,
            UpdatedDateUtc = DateTime.UtcNow
        });
}
