// Copyright (c) ABCDEG. All rights reserved.

using Billing.Invoices.Contracts.Models;

namespace Billing.Invoices.Contracts.IntegrationEvents;

[EventTopic<Invoice>]
public record InvoicePaid
{
    public string TenantId { get; set; } = string.Empty;
    public Guid InvoiceId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal AmountPaid { get; set; }
    public DateTime PaymentDate { get; set; }
}
