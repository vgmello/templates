// Copyright (c) ABCDEG. All rights reserved.

using Billing.Invoices.Commands;
using Billing.Invoices.Contracts.IntegrationEvents;

namespace Billing.BackOffice.Messaging.BillingInboxHandler;

public static class PaymentReceivedHandler
{
    public static async Task Handle(PaymentReceived @event, IMessageBus messaging, CancellationToken cancellationToken)
    {
        var markPaidCommand = new MarkInvoiceAsPaidCommand(
            @event.InvoiceId,
            @event.Amount,
            @event.ReceivedDate
        );

        await messaging.InvokeCommandAsync(markPaidCommand, cancellationToken);
    }
}
