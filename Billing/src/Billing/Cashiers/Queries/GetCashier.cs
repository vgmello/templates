// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashiers.Contracts.Models;
using Dapper;
using Npgsql;

namespace Billing.Cashiers.Queries;

public record GetCashierQuery(Guid Id) : IQuery<Cashier>;

/// <summary>
///     Example of query handler with db query directly in the handler, with DbCommand attr with custom column.
///     It works, but it is not really recommended to have the query directly in the main handler, it makes harder to test
/// </summary>
public static partial class GetCashierQueryHandler
{
    [DbCommand]
    private sealed partial record DbCommand([Column("id")] Guid CashierId);

    public static async Task<Cashier> Handle(GetCashierQuery query, NpgsqlDataSource dataSource, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = "SELECT cashier_id, name, email FROM billing.cashiers WHERE cashier_id = @id";

        var cashier = await connection.QuerySingleOrDefaultAsync<Data.Entities.Cashier>(sql, new DbCommand(query.Id).ToDbParams());

        if (cashier is null)
        {
            throw new InvalidOperationException($"Cashier with ID {query.Id} not found");
        }

        return new Cashier
        {
            CashierId = cashier.CashierId,
            Name = cashier.Name,
            Email = cashier.Email ?? string.Empty
        };
    }
}
