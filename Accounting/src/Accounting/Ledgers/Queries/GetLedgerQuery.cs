// Copyright (c) ABCDEG. All rights reserved.

using Dapper;
using Npgsql;

namespace Accounting.Ledgers.Queries;

public record GetLedgerQuery(Guid Id);

public static class GetLedgerQueryHandler
{
    public static async Task<Contracts.Ledgers.Models.Ledger?> Handle(GetLedgerQuery query, NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                LedgerBalanceId AS LedgerId,
                ClientId,
                LedgerType,
                BalanceDate
            FROM LedgerBalances
            WHERE LedgerBalanceId = @Id
            """;
        return await connection.QuerySingleOrDefaultAsync<Contracts.Ledgers.Models.Ledger>(sql, new { query.Id });
    }
}
