// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.Invoices.Contracts.IntegrationEvents;

public record InvoicePaid
{
    public string TenantId { get; set; } = string.Empty;
    public Guid InvoiceId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal AmountPaid { get; set; }
    public DateTime PaymentDate { get; set; }
}
