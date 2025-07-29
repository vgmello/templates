// Copyright (c) ABCDEG. All rights reserved.

using Billing.Core.Data;
using Billing.Invoices.Contracts.Models;
using Billing.Invoices.Data;
using FluentValidation.Results;
using LinqToDB;

namespace Billing.Invoices.Queries;

public record GetInvoiceQuery(Guid TenantId, Guid Id) : IQuery<Result<Invoice>>;

public static class GetInvoiceQueryHandler
{
    public static async Task<Result<Invoice>> Handle(GetInvoiceQuery query, BillingDb db, CancellationToken cancellationToken)
    {
        var invoice = await db.Invoices
            .FirstOrDefaultAsync(i => i.TenantId == query.TenantId && i.InvoiceId == query.Id, cancellationToken);

        if (invoice is null)
        {
            return new List<ValidationFailure> { new("Id", "Invoice not found") };
        }

        return invoice.ToModel();
    }
}
