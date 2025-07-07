// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.Invoices.Contracts.DomainEvents;

public class InvoiceGenerated
{
    public Guid InvoiceId { get; set; }
    public decimal InvoiceAmount { get; set; }
}
