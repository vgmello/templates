// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.BackOffice.Orleans.Grains;

public interface IInvoiceGrain : IGrainWithGuidKey
{
    Task<InvoiceState> GetState();
    Task Pay(decimal amount);
}
