// Copyright (c) ABCDEG. All rights reserved.

using Billing.Invoices.Contracts.Models;

namespace Billing.Invoices.Queries;

using InvoiceModel = Invoice;

public record GetInvoicesQuery(int Limit = 50, int Offset = 0, string? Status = null) : IQuery<IEnumerable<InvoiceModel>>;

public static partial class GetInvoicesQueryHandler
{
    [DbCommand(fn: "select * from billing.invoices_get")]
    public partial record GetInvoicesDbQuery(int Limit, int Offset, string? Status) : IQuery<IEnumerable<InvoiceModel>>;

    public static async Task<IEnumerable<InvoiceModel>> Handle(GetInvoicesQuery query, IMessageBus messaging,
        CancellationToken cancellationToken)
    {
        var dbQuery = new GetInvoicesDbQuery(query.Limit, query.Offset, query.Status);

        var invoices = await messaging.InvokeQueryAsync(dbQuery, cancellationToken);

        return invoices;
    }
}
