// Copyright (c) ABCDEG. All rights reserved.

using Billing.Core.Data;

namespace Billing.Cashiers.Data.Entities;

public record Cashier : Entity
{
    public Guid CashierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
}
