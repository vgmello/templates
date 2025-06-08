// Copyright (c) ABCDEG. All rights reserved.

using Orleans.Serialization;

namespace Accounting.BackOffice.Orleans.Grains;

[GenerateSerializer]
public sealed class InvoiceState
{
    [Id(0)]
    public decimal Amount { get; set; }

    [Id(1)]
    public bool Paid { get; set; }
}
