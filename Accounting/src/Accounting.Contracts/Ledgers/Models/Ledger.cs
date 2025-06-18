// Copyright (c) ABCDEG. All rights reserved.

namespace Accounting.Contracts.Ledgers.Models;

public record Ledger
{
    public Guid LedgerId { get; set; }

    public Guid ClientId { get; set; }

    public LedgerType LedgerType { get; set; }

    public List<LedgerPayment> LedgerPayments { get; set; } = new();
}
