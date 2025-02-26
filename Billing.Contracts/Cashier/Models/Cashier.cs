// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.Contracts.Cashier.Models;

public record Cashier
{
    public int CashierId { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    public List<CashierPayment> CashierPayments { get; set; }
}
