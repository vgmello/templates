// Copyright (c) ABCDEG. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Accounting.Contracts.Ledgers.Models;

namespace Accounting.Ledgers.Queries;

public record GetLedgersQuery
{
    [Range(1, 1000)]
    public int Limit { get; set; } = 1000;

    [Range(0, int.MaxValue)]
    public int Offset { get; set; } = 0;

    public record Result(Guid LedgerId, Guid ClientId, LedgerType LedgerType);
}

public static class GetLedgersQueryHandler
{
    public static async Task<IEnumerable<GetLedgersQuery.Result>> Handle(GetLedgersQuery query, CancellationToken cancellationToken)
    {
        return new[]
        {
            new GetLedgersQuery.Result(Guid.NewGuid(), Guid.NewGuid(), LedgerType.Cash),
            new GetLedgersQuery.Result(Guid.NewGuid(), Guid.NewGuid(), LedgerType.Payable)
        };
    }
}
