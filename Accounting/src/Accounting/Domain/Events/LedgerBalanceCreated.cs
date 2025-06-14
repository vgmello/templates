// Copyright (c) ABCDEG. All rights reserved.

using System;

namespace Accounting.Domain.Events;

/// <summary>
/// Domain event raised when a new ledger balance is created.
/// This event is typically handled within the Accounting service.
/// </summary>
public class LedgerBalanceCreated
{
    public Guid LedgerId { get; init; }
    public decimal FinalBalance { get; init; }
    public DateTimeOffset AsOfDate { get; init; }

    // Parameterless constructor for Wolverine/deserialization if needed,
    // though init-only properties with a parameterized constructor are often preferred.
    public LedgerBalanceCreated(Guid ledgerId, decimal finalBalance, DateTimeOffset asOfDate)
    {
        LedgerId = ledgerId;
        FinalBalance = finalBalance;
        AsOfDate = asOfDate;
    }
}
