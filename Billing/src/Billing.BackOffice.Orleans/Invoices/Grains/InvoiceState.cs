// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.BackOffice.Orleans.Invoices.Grains;

[GenerateSerializer]
public sealed class InvoiceState
{
    [Id(0)]
    public decimal Amount { get; set; }

    [Id(1)]
    public bool Paid { get; set; }
}
