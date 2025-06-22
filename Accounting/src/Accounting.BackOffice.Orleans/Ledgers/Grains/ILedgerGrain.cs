// Copyright (c) ABCDEG. All rights reserved.

namespace Accounting.BackOffice.Orleans.Ledgers.Grains;

public interface ILedgerGrain : IGrainWithGuidKey
{
    Task<LedgerState> GetState();
    Task Pay(decimal amount);
}
