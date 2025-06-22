// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.Contracts.Cashier.Models;

public record Cashier
{
    public string TenantId { get; set; } = string.Empty;
    public Guid CashierId { get; set; }

    public required string Name { get; set; }

    public required string Email { get; set; }

    public List<CashierPayment> CashierPayments { get; set; } = [];
}
