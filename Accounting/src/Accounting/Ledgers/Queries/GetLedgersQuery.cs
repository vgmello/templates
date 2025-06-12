// Copyright (c) ABCDEG. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Accounting.Contracts.Ledgers.Models;
using Dapper;
using Npgsql;

namespace Accounting.Ledgers.Queries;

public record GetLedgersQuery
{
    [Range(1, 1000)]
    public int Limit { get; set; } = 1000;

    [Range(0, int.MaxValue)]
    public int Offset { get; set; } = 0;

    public record Result(Guid LedgerId, Guid ClientId, LedgerType LedgerType);
}

public static class GetLedgersQueryHandler
{
    public static async Task<IEnumerable<GetLedgersQuery.Result>> Handle(
        GetLedgersQuery query,
        NpgsqlConnection connection,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                LedgerBalanceId AS LedgerId,
                ClientId,
                LedgerType
            FROM LedgerBalances
            ORDER BY LedgerBalanceId -- Or any other consistent ordering column
            LIMIT @Limit OFFSET @Offset
            """;

        return await connection.QueryAsync<GetLedgersQuery.Result>(
            sql,
            new { query.Limit, query.Offset });
    }
}
