// Copyright (c) ABCDEG. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Dapper;
using Npgsql;

namespace Billing.Cashier.Queries;

public record GetCashiersQuery : IQuery<IEnumerable<GetCashiersQuery.Result>>
{
    [Range(1, 1000)]
    public int Limit { get; set; } = 1000;

    [Range(0, int.MaxValue)]
    public int Offset { get; set; } = 0;

    public record Result(Guid CashierId, string Name, string Email);
}

public static class GetCashiersQueryHandler
{
    public static async Task<IEnumerable<GetCashiersQuery.Result>> Handle(GetCashiersQuery query,
        NpgsqlDataSource dataSource, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var sql = @"
            SELECT cashier_id, name
            FROM billing.cashiers
            ORDER BY created_date_utc DESC
            LIMIT @Limit OFFSET @Offset";

        var cashiers = await connection.QueryAsync<GetCashiersQuery.Result>(
            sql, new { query.Limit, query.Offset });

        return cashiers;
    }
}
