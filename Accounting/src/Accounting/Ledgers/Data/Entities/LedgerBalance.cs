// Copyright (c) ABCDEG. All rights reserved.

using Accounting.Core.Data;
using Accounting.Contracts.Ledgers.Models;

namespace Accounting.Ledgers.Data.Entities;

public record LedgerBalance : Entity
{
    public Guid LedgerBalanceId { get; set; }

    public Guid ClientId { get; set; }

    public LedgerType LedgerType { get; set; }

    public DateOnly BalanceDate { get; set; }
}
