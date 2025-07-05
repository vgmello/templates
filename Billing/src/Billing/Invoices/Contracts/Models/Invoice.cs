// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.Invoices.Contracts.Models;

public record Invoice
{
    public Guid InvoiceId { get; set; }

    public required string Name { get; set; }

    public required string Status { get; set; }

    public decimal Amount { get; set; }

    public string? Currency { get; set; }

    public DateTime? DueDate { get; set; }

    public Guid? CashierId { get; set; }

    public DateTime CreatedDateUtc { get; set; }

    public DateTime UpdatedDateUtc { get; set; }

    public int Version { get; set; }
}
