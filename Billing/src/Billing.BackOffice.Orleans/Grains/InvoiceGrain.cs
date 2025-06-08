// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.BackOffice.Orleans.Grains;

public sealed class InvoiceGrain(
    [PersistentState("invoice", "Default")]
    IPersistentState<InvoiceState> state)
    : Grain, IInvoiceGrain
{
    public Task<InvoiceState> GetState() => Task.FromResult(state.State);

    public async Task Pay(decimal amount)
    {
        state.State.Amount += amount;
        state.State.Paid = true;
        await state.WriteStateAsync();
    }
}
