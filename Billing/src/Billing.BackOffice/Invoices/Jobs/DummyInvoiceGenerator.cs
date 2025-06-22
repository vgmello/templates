// Copyright (c) ABCDEG. All rights reserved.

using Billing.Contracts.Invoices.IntegrationEvents;

namespace Billing.BackOffice.Operations.Jobs;

public class DummyInvoiceGenerator(IMessageBus bus) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await bus.PublishAsync(new InvoicePaidEvent());

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
