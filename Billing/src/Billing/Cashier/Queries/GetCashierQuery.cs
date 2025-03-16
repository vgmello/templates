// Copyright (c) ABCDEG. All rights reserved.

using System.Data;

namespace Billing.Cashier.Queries;

public record GetCashierQuery(Guid Id);

public static class GetCashierQueryHandler
{
    public static async Task<Contracts.Cashier.Models.Cashier> Handle(GetCashierQuery query,
        IDbConnection connection,
        CancellationToken cancellationToken)
    {
        return new Contracts.Cashier.Models.Cashier
        {
            CashierId = query.Id
        };
    }
}
