// Copyright (c) ABCDEG. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Billing.Cashier.Data.Persistence;

namespace Billing.Cashier.Queries;

public record GetCashiersQuery : IRequest<IEnumerable<GetCashiersQuery.Result>>
{
    [Range(1, 1000)]
    public int Limit { get; set; } = 1000;

    [Range(0, int.MaxValue)]
    public int Offset { get; set; } = 0;

    public record Result(Guid CashierId, string Name);
}

public class GetCashiersQueryHandler(IQueryServices services) :
    QueryHandler<GetCashiersQuery, IEnumerable<GetCashiersQuery.Result>>(services)
{
    protected override async Task<IEnumerable<GetCashiersQuery.Result>> Handle(GetCashiersQuery query)
    {
        var cashiers = await SendQuery(new GetCashiersDbQuery
        {
            Offset = query.Offset,
            Limit = query.Limit,
        });

        return cashiers.Select(c => new GetCashiersQuery.Result(c.CashierId, c.Name));
    }
}
