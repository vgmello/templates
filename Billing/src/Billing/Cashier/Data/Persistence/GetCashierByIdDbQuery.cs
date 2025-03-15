// Copyright (c) ABCDEG. All rights reserved.

using Billing.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Billing.Cashier.Data.Persistence;

public record GetCashierByIdDbQuery(Guid Id) : IRequest<Entities.Cashier?>;

public class GetCashierByIdQueryHandler(BillingDbContext context)
    : IRequestHandler<GetCashierByIdDbQuery, Entities.Cashier?>
{
    public async Task<Entities.Cashier?> Handle(GetCashierByIdDbQuery request, CancellationToken cancellationToken)
    {
        var result = await context.Cashiers.SingleOrDefaultAsync(m => m.CashierId == request.Id, cancellationToken);

        return result;
    }
}
