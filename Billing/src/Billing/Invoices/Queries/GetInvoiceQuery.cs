// Copyright (c) ABCDEG. All rights reserved.

using FluentValidation.Results;

namespace Billing.Invoices.Queries;

using InvoiceModel = Billing.Contracts.Invoices.Models.Invoice;

public record GetInvoiceQuery(Guid Id) : IQuery<Result<InvoiceModel>>;

public static partial class GetInvoiceQueryHwandler
{
    [DbCommand(sp: "billing.invoice_get")]
    public partial record GetInvoiceDbQuery(Guid InvoiceId) : IQuery<InvoiceModel?>;

    public static async Task<Result<InvoiceModel>> Handle(GetInvoiceQuery query, IMessageBus messaging, CancellationToken cancellationToken)
    {
        var dbQuery = new GetInvoiceDbQuery(query.Id);

        var invoice = await messaging.InvokeQueryAsync(dbQuery, cancellationToken);

        if (invoice is not null)
            return invoice;

        return new List<ValidationFailure> { new("Id", "Invoice not found") };
    }
}
