// Copyright (c) ABCDEG. All rights reserved.

namespace Accounting.Ledgers.Events;

public class LedgerBalanceCreated
{
    public Guid LedgerId { get; set; }
    public decimal FinalBalance { get; set; }
    public DateOnly AsOfDate { get; set; }
}