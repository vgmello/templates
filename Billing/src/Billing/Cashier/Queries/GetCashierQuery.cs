// Copyright (c) ABCDEG. All rights reserved.

using Dapper;
using Npgsql;

namespace Billing.Cashier.Queries;

public record GetCashierQuery(Guid Id) : IQuery<Contracts.Cashier.Models.Cashier>;

/// <summary>
///     Example of query handler with db query directly in the handler, with DbCommand attr with custom column.
/// </summary>
public static partial class GetCashierQueryHandler
{
    [DbCommand]
    private sealed partial record DbCommand([Column("id")] Guid CashierId);

    public static async Task<Contracts.Cashier.Models.Cashier> Handle(GetCashierQuery query, NpgsqlDataSource dataSource,
        CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
                               SELECT cashier_id AS CashierId, name AS Name, email AS Email, created_date_utc, updated_date_utc, version
                               FROM billing.cashiers
                               WHERE cashier_id = @id
                           """;

        var cashier = await connection.QuerySingleOrDefaultAsync<Data.Entities.Cashier>(sql, new DbCommand(query.Id).ToDbParams());

        if (cashier is null)
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
