// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.Invoices.Events;

public class InvoiceGenerated
{
    public Guid InvoiceId { get; set; }
    public decimal InvoiceAmount { get; set; }
}
