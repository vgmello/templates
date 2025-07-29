// Copyright (c) ABCDEG. All rights reserved.

using Billing.Core.Data;
using Billing.Invoices.Contracts.Models;
using Billing.Invoices.Data;
using LinqToDB;

namespace Billing.Invoices.Queries;

public record GetInvoicesQuery(Guid TenantId, int Offset = 0, int Limit = 100, string? Status = null)
    : IQuery<IEnumerable<Invoice>>;

public static class GetInvoicesQueryHandler
{
    public static async Task<IEnumerable<Invoice>> Handle(GetInvoicesQuery query, BillingDb db, CancellationToken cancellationToken)
    {
        var queryable = db.Invoices
            .Where(i => i.TenantId == query.TenantId);

        if (!string.IsNullOrEmpty(query.Status))
        {
            queryable = queryable.Where(i => i.Status == query.Status);
        }

        var invoices = await queryable
            .OrderByDescending(i => i.CreatedDateUtc)
            .Skip(query.Offset)
            .Take(query.Limit)
            .Select(i => i.ToModel())
            .ToListAsync(cancellationToken);

        return invoices;
    }
}
