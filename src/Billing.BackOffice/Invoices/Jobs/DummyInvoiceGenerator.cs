// Copyright (c) ABCDEG. All rights reserved.

using Billing.Invoices.Contracts.IntegrationEvents;
using Billing.Invoices.Contracts.Models;

namespace Billing.BackOffice.Invoices.Jobs;

public class DummyInvoiceGenerator(IMessageBus bus) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await bus.PublishAsync(new InvoicePaid(
                Guid.Empty,
                new Invoice { Name = "Fake Invoice", Status = "Paid" }
            ));

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
