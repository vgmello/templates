// Copyright (c) ABCDEG. All rights reserved.

namespace Accounting.BackOffice.Orleans.Grains;

[GenerateSerializer]
public sealed class LedgerState
{
    [Id(0)]
    public decimal Amount { get; set; }

    [Id(1)]
    public bool Paid { get; set; }
}
