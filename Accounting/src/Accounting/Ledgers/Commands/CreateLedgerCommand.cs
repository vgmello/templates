// Copyright (c) ABCDEG. All rights reserved.

using Accounting.Contracts.Ledgers.IntegrationEvents;
using Accounting.Contracts.Ledgers.Models;
using LedgerEntity = Accounting.Ledgers.Data.Entities.LedgerBalance;
using LedgerModel = Accounting.Contracts.Ledgers.Models.Ledger;

namespace Accounting.Ledgers.Commands;

public record CreateLedgerCommand(Guid ClientId, LedgerType LedgerType) : ICommand<Result<LedgerModel>>;

public class CreateCustomerValidator : AbstractValidator<CreateLedgerCommand>
{
    public CreateCustomerValidator()
    {
        RuleFor(c => c.ClientId).NotEmpty();
    }
}

public static class CreateLedgerCommandHandler
{
    public record InsertLedgerCommand(LedgerEntity Ledger) : ICommand<int>;

    public static async Task<(Result<LedgerModel>, LedgerCreatedEvent)> Handle(
        CreateLedgerCommand command, IMessageContext messaging, CancellationToken cancellationToken)
    {
        var entity = new LedgerEntity
        {
            LedgerBalanceId = Guid.NewGuid(),
            ClientId = command.ClientId,
            LedgerType = command.LedgerType,
            BalanceDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        await messaging.InvokeCommandAsync(new InsertLedgerCommand(entity), cancellationToken);

        var result = new LedgerModel
        {
            LedgerId = entity.LedgerBalanceId,
            ClientId = entity.ClientId,
            LedgerType = entity.LedgerType
        };

        return (result, new LedgerCreatedEvent(result));
    }

    public static Task<int> Handle(InsertLedgerCommand command, CancellationToken cancellationToken)
    {
        return Task.FromResult(1);
    }
}
