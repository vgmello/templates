// Copyright (c) ABCDEG. All rights reserved.

namespace Accounting.BackOffice.Orleans.Ledgers.Grains;

public sealed class LedgerGrain([PersistentState("ledger")] IPersistentState<LedgerState> state) : Grain, ILedgerGrain
{
    public Task<LedgerState> GetState() => Task.FromResult(state.State);

    public async Task Pay(decimal amount)
    {
        state.State.Amount += amount;
        state.State.Paid = true;
        await state.WriteStateAsync();
    }
}
