// Copyright (c) ABCDEG. All rights reserved.

using System;

namespace Billing.Domain.Events;

/// <summary>
/// Domain event raised when a new invoice is generated internally.
/// This event is typically handled within the Billing service.
/// </summary>
public class InvoiceGenerated
{
    public Guid InvoiceId { get; init; }
    public decimal Amount { get; init; }

    public InvoiceGenerated(Guid invoiceId, decimal amount)
    {
        InvoiceId = invoiceId;
        Amount = amount;
    }
}
