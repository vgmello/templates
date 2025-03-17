// Copyright (c) ABCDEG. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Billing.Cashier.Queries;

public record GetCashiersQuery
{
    [Range(1, 1000)]
    public int Limit { get; set; } = 1000;

    [Range(0, int.MaxValue)]
    public int Offset { get; set; } = 0;

    public record Result(Guid CashierId, string Name);
}

public static class GetCashiersQueryHandler
{
    public static async Task<IEnumerable<GetCashiersQuery.Result>> Handle(GetCashiersQuery query,
        CancellationToken cancellationToken)
    {
        return [];
    }
}
