// Copyright (c) ABCDEG. All rights reserved.

using Billing.Invoices.Contracts.IntegrationEvents;

namespace Billing.BackOffice.Invoices.Jobs;

public class DummyInvoiceGenerator(IMessageBus bus) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await bus.PublishAsync(new InvoicePaid
            {
                InvoiceId = Guid.NewGuid(),
                AmountPaid = Random.Shared.Next(1, 100),
                CustomerId = Guid.NewGuid(),
                PaymentDate = DateTime.UtcNow
            });

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
