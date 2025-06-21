// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.Contracts.Cashier.Models;

public record CashierPayment
{
    public int CashierId { get; set; }

    public int PaymentId { get; set; }

    public DateTime PaymentDate { get; set; }
}
