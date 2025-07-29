// Copyright (c) ABCDEG. All rights reserved.

using Billing.Core.Data;
using LinqToDB.Mapping;

namespace Billing.Cashiers.Data.Entities;

public record Cashier : DbEntity
{
    [PrimaryKey(order: 0)]
    public Guid TenantId { get; set; }

    [PrimaryKey(order: 1)]
    public Guid CashierId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Email { get; set; }
}
