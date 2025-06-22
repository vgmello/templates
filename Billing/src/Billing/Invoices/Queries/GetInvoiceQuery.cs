// Copyright (c) ABCDEG. All rights reserved.

using Dapper;
using Npgsql;

namespace Billing.Invoices.Queries;

using InvoiceModel = Billing.Contracts.Invoices.Models.Invoice;

public record GetInvoiceQuery(Guid Id) : IQuery<InvoiceModel>;

public static class GetInvoiceQueryHandler
{
    public static async Task<InvoiceModel> Handle(GetInvoiceQuery query, NpgsqlDataSource dataSource, CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT invoice_id, name, status, amount, currency, due_date, cashier_id,
                                  created_date_utc, updated_date_utc, version
                           FROM billing.invoices
                           WHERE invoice_id = @Id
                           """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var invoice = await connection.QuerySingleOrDefaultAsync<InvoiceModel>(sql, new { query.Id });

        return invoice ?? throw new InvalidOperationException($"Invoice with ID {query.Id} not found");
    }
}
