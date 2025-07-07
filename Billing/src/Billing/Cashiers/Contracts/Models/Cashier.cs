// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.Cashiers.Contracts.Models;

public record Cashier
{
    public Guid TenantId { get; set; }

    public Guid CashierId { get; set; }

    public required string Name { get; set; }

    public required string Email { get; set; }

    public List<CashierPayment> CashierPayments { get; set; } = [];
}
