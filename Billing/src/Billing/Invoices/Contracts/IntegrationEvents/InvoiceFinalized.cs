// Copyright (c) ABCDEG. All rights reserved.

using Billing.Invoices.Contracts.Models;

namespace Billing.Invoices.Contracts.IntegrationEvents;

[EventTopic<Invoice>]
public class InvoiceFinalized
{
    public string TenantId { get; set; } = string.Empty;
    public Guid InvoiceId { get; set; }
    public Guid CustomerId { get; set; }
    public string PublicInvoiceNumber { get; set; } = string.Empty;
    public decimal FinalTotalAmount { get; set; }
}
