// Copyright (c) ABCDEG. All rights reserved.

using Billing.Infrastructure.Database;

namespace Billing.Cashier.Data.Persistence;

public record AddCashierDbCommand(Entities.Cashier Cashier) : IRequest<Guid>;

public class AddCashierDbCommandHandler(BillingDbContext context) : IRequestHandler<AddCashierDbCommand, Guid>
{
    public async Task<Guid> Handle(AddCashierDbCommand request, CancellationToken cancellationToken)
    {
        context.Cashiers.Add(request.Cashier);

        await context.SaveChangesAsync(cancellationToken);

        return request.Cashier.CashierId;
    }
}
