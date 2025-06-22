// Copyright (c) ABCDEG. All rights reserved.

using Billing.Contracts.Invoices.IntegrationEvents;

namespace Billing.BackOffice.Invoices.Jobs;

public class DummyInvoiceGenerator(IMessageBus bus) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await bus.PublishAsync(new InvoicePaidEvent
            {
                InvoiceId = Guid.NewGuid(),
                AmountPaid = Random.Shared.Next(1, 100),
                CustomerId = Guid.NewGuid(),
                PaymentDate = DateTime.UtcNow,
            });

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
