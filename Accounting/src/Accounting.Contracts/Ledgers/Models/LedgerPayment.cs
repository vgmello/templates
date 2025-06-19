// Copyright (c) ABCDEG. All rights reserved.

namespace Accounting.Contracts.Ledgers.Models;

public record LedgerPayment
{
    public int LedgerId { get; set; }

    public int PaymentId { get; set; }

    public DateTime PaymentDate { get; set; }
}
