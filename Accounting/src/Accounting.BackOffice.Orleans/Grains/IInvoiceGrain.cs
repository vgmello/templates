// Copyright (c) ABCDEG. All rights reserved.

using Orleans;

namespace Accounting.BackOffice.Orleans.Grains;

public interface IInvoiceGrain : IGrainWithGuidKey
{
    Task<InvoiceState> GetState();
    Task Pay(decimal amount);
}
