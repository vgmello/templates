// Copyright (c) ABCDEG. All rights reserved.

using Dapper;
using Npgsql;

namespace Billing.Cashier.Queries;

public record GetCashierQuery(Guid Id);

public static class GetCashierQueryHandler
{
    public static async Task<Contracts.Cashier.Models.Cashier> Handle(GetCashierQuery query,
        NpgsqlDataSource dataSource, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var sql = @"
            SELECT cashier_id, name, email, created_date_utc, updated_date_utc, version
            FROM billing.cashiers 
            WHERE cashier_id = @CashierId";

        var cashier = await connection.QuerySingleOrDefaultAsync<Data.Entities.Cashier>(
            sql, new { CashierId = query.Id });

        if (cashier == null)
        {
            throw new InvalidOperationException($"Cashier with ID {query.Id} not found");
        }

        return new Contracts.Cashier.Models.Cashier
        {
            CashierId = cashier.CashierId,
            Name = cashier.Name,
            Email = cashier.Email ?? string.Empty
        };
    }
}
