// Copyright (c) ABCDEG. All rights reserved.

using Billing.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Billing.Cashier.Data.Persistence;

public record GetCashiersDbQuery(int Offset = 0, int Limit = 1000) : IRequest<ICollection<Entities.Cashier>>;

public class GetCashiersDbQueryHandler(BillingDbContext context) :
    IRequestHandler<GetCashiersDbQuery, ICollection<Entities.Cashier>>
{
    public async Task<ICollection<Entities.Cashier>> Handle(GetCashiersDbQuery request, CancellationToken cancellationToken)
    {
        var result = await context.Cashiers
            .Skip(request.Offset)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        return result;
    }
}
