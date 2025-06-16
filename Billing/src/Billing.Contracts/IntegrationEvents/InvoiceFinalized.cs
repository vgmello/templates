// Copyright (c) ABCDEG. All rights reserved.

using System;

namespace Billing.Contracts.IntegrationEvents;

/// <summary>
/// Integration event raised when an invoice has been finalized and is ready for external communication.
/// This event is published to external services (e.g., via Kafka).
/// </summary>
public class InvoiceFinalized
{
    public Guid InvoiceId { get; init; }
    public Guid CustomerId { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }

    public InvoiceFinalized(Guid invoiceId, Guid customerId, string invoiceNumber, decimal totalAmount)
    {
        InvoiceId = invoiceId;
        CustomerId = customerId;
        InvoiceNumber = invoiceNumber;
        TotalAmount = totalAmount;
    }
}
