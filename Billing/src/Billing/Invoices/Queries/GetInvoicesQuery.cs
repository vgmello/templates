// Copyright (c) ABCDEG. All rights reserved.

using Dapper;
using Npgsql;

namespace Billing.Invoices.Queries;

using InvoiceModel = Billing.Contracts.Invoices.Models.Invoice;

public record GetInvoicesQuery(int Limit = 50, int Offset = 0, string? Status = null) : IQuery<IEnumerable<InvoiceModel>>;

public static class GetInvoicesQueryHandler
{
    public static async Task<IEnumerable<InvoiceModel>> Handle(GetInvoicesQuery query,
        NpgsqlDataSource dataSource, CancellationToken cancellationToken)
    {
        var sql = """
            SELECT invoice_id, name, status, amount, currency, due_date, cashier_id,
                   created_date_utc, updated_date_utc, version
            FROM billing.invoices
            """;

        var parameters = new DynamicParameters();
        
        if (!string.IsNullOrEmpty(query.Status))
        {
            sql += " WHERE status = @Status";
            parameters.Add("Status", query.Status);
        }

        sql += " ORDER BY created_date_utc DESC LIMIT @Limit OFFSET @Offset";
        parameters.Add("Limit", query.Limit);
        parameters.Add("Offset", query.Offset);

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var invoices = await connection.QueryAsync<InvoiceModel>(sql, parameters);

        return invoices;
    }
}