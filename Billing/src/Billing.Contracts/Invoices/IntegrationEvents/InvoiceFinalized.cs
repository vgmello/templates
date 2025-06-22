// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.Contracts.Invoices.IntegrationEvents;

public class InvoiceFinalized
{
    public Guid InvoiceId { get; set; }
    public Guid CustomerId { get; set; }
    public string PublicInvoiceNumber { get; set; } = string.Empty;
    public decimal FinalTotalAmount { get; set; }
}