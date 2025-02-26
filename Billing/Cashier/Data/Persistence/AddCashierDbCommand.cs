// Copyright (c) ABCDEG. All rights reserved.

using Billing.Infrastructure.Database;
using MediatR;

namespace Billing.Cashier.Data.Persistence;

public class AddCashierDbCommand : IRequest
{
    public required Entities.Cashier Cashier { get; set; }
}

public class AddCashierDbCommandHandler(BillingDbContext context) : IRequestHandler<AddCashierDbCommand>
{
    public async Task Handle(AddCashierDbCommand request, CancellationToken cancellationToken)
    {
        context.Cashiers.Add(request.Cashier);

        await context.SaveChangesAsync(cancellationToken);
    }
}
