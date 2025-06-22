// Copyright (c) ABCDEG. All rights reserved.

using Billing.Contracts.Invoices.IntegrationEvents;

namespace Billing.BackOffice.Invoices.Jobs;

public class DummyInvoiceGenerator(IMessageBus bus) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await bus.PublishAsync(new InvoicePaidEvent(Guid.CreateVersion7(), Random.Shared.NextInt64(), DateTime.UtcNow));

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
