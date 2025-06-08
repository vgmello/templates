// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.Cashier.Queries;

public record GetCashierQuery(Guid Id);

public static class GetCashierQueryHandler
{
    public static async Task<Contracts.Cashier.Models.Cashier> Handle(GetCashierQuery query,
        CancellationToken cancellationToken)
    {
        return new Contracts.Cashier.Models.Cashier
        {
            CashierId = query.Id,
            Name = "Test",
            Email = "test@example.com"
        };
    }
}
