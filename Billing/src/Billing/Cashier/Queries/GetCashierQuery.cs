// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashier.Data.Persistence;

namespace Billing.Cashier.Queries;

public record GetCashierQuery(Guid Id) : IRequest<Contracts.Cashier.Models.Cashier?>;

public class GetCashierQueryHandler(IQueryServices services) :
    QueryHandler<GetCashierQuery, Contracts.Cashier.Models.Cashier?>(services)
{
    protected override async Task<Contracts.Cashier.Models.Cashier?> Handle(GetCashierQuery request)
    {
        var cashier = await SendQuery(new GetCashierByIdDbQuery(request.Id));

        if (cashier is null)
            return null;

        return new Contracts.Cashier.Models.Cashier
        {
            CashierId = cashier.CashierId,
            Name = cashier.Name
        };
    }
}
