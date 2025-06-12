// Copyright (c) ABCDEG. All rights reserved.

using Accounting.Contracts.Ledgers.IntegrationEvents;
using Accounting.Contracts.Ledgers.Models;
using Dapper;
using Npgsql;
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
        // The LedgerBalance constructor now handles ID generation and sets properties.
        var entity = new LedgerEntity(
            command.ClientId,
            command.LedgerType,
            DateOnly.FromDateTime(DateTime.UtcNow)
        );

        await messaging.InvokeCommandAsync(new InsertLedgerCommand(entity), cancellationToken);

        var result = new LedgerModel
        {
            LedgerId = entity.LedgerBalanceId,
            ClientId = entity.ClientId,
            LedgerType = entity.LedgerType
        };

        return (result, new LedgerCreatedEvent(result));
    }

    public static async Task<int> Handle(InsertLedgerCommand command, NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO LedgerBalances (LedgerBalanceId, ClientId, LedgerType, BalanceDate)
            VALUES (@LedgerBalanceId, @ClientId, @LedgerType, @BalanceDate)
            """;
        return await connection.ExecuteAsync(sql, command.Ledger);
    }
}
