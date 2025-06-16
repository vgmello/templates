// Copyright (c) ABCDEG. All rights reserved.

namespace Accounting.Ledgers.Queries;

public record GetLedgerQuery(Guid Id);

public static class GetLedgerQueryHandler
{
    public static async Task<Contracts.Ledgers.Models.Ledger> Handle(GetLedgerQuery query, CancellationToken cancellationToken)
    {
        return new Contracts.Ledgers.Models.Ledger
        {
            LedgerId = query.Id,
            ClientId = Guid.NewGuid(),
            LedgerType = Contracts.Ledgers.Models.LedgerType.Cash
        };
    }
}
