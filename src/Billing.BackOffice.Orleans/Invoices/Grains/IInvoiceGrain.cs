// Copyright (c) ABCDEG. All rights reserved.

using Orleans.Concurrency;

namespace Billing.BackOffice.Orleans.Invoices.Grains;

public interface IInvoiceGrain : IGrainWithGuidKey
{
    Task<InvoiceState> GetState();

    Task Pay(decimal amount);

    [OneWay]
    Task Notify(bool important);
}
