// Copyright (c) ABCDEG. All rights reserved.

using Dapper;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace Billing.Cashiers.Queries;

public record GetCashiersQuery : IQuery<IEnumerable<GetCashiersQuery.Result>>
{
    [Range(1, 1000)]
    public int Limit { get; set; } = 1000;

    [Range(0, int.MaxValue)]
    public int Offset { get; set; }

    public record Result(Guid TenantId, Guid CashierId, string Name, string Email);
}

/// <summary>
///     Example of query handler with db query directly in the handler, with DbCommand attr just for snake case conversion.
/// </summary>
public static partial class GetCashiersQueryHandler
{
    [DbCommand]
    private sealed partial record DbCommand(int Limit, int Offset);

    public static async Task<IEnumerable<GetCashiersQuery.Result>> Handle(GetCashiersQuery query, NpgsqlDataSource dataSource,
        CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
                               SELECT null::uuid as tenant_id, cashier_id, name, email
                               FROM billing.cashiers
                               LIMIT @limit OFFSET @offset
                           """;

        var cashiers = await connection.QueryAsync<GetCashiersQuery.Result>(sql, new DbCommand(query.Limit, query.Offset).ToDbParams());

        return cashiers;
    }
}
