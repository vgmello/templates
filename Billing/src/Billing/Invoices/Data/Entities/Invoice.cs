// Copyright (c) ABCDEG. All rights reserved.

using Billing.Core.Data;

namespace Billing.Invoices.Data.Entities;

public record Invoice : Entity
{
    public Guid InvoiceId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string? Currency { get; set; }

    public DateTime? DueDate { get; set; }

    public Guid? CashierId { get; set; }
}
