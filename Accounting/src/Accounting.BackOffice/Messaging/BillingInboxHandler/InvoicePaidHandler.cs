// Copyright (c) ABCDEG. All rights reserved.

using Billing.Contracts.Invoices.IntegrationEvents;

namespace Accounting.BackOffice.Messaging.BillingInboxHandler;

public static class InvoicePaidHandler
{
    public static async Task Handle(InvoicePaid @event, IMessageBus messaging, ILogger<InvoicePaid> logger,
        CancellationToken cancellationToken)
    {
        // Handle the InvoicePaid event here

        logger.LogInformation("Invoice paid received from Billing: {InvoiceId}", @event.InvoiceId);

        await Task.CompletedTask;
    }
}
