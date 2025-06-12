// Copyright (c) ABCDEG. All rights reserved.

using Accounting.Core.Data;
using Accounting.Contracts.Ledgers.Models;

namespace Accounting.Ledgers.Data.Entities;

public record LedgerBalance : Entity
{
    public Guid LedgerBalanceId { get; private set; }

    public Guid ClientId { get; private set; }

    public LedgerType LedgerType { get; private set; }

    public DateOnly BalanceDate { get; private set; }

    public LedgerBalance(Guid clientId, LedgerType ledgerType, DateOnly balanceDate)
    {
        LedgerBalanceId = Guid.NewGuid();
        ClientId = clientId;
        LedgerType = ledgerType;
        BalanceDate = balanceDate;
    }
}
