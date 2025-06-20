// Copyright (c) ABCDEG. All rights reserved.

using Billing.Contracts.Invoices.IntegrationEvents;
using Billing.Invoices.Commands;

namespace Billing.BackOffice.Messaging.IntegrationEvents.Invoices;

public static class PaymentReceivedEventHandler
{
    public static async Task Handle(
        PaymentReceivedEvent @event,
        IMessageBus messaging,
        CancellationToken cancellationToken)
    {
        // When a payment is received, automatically mark the invoice as paid
        var markPaidCommand = new MarkInvoiceAsPaidCommand(
            @event.InvoiceId,
            @event.Amount,
            @event.ReceivedDate
        );

        // Send the command to mark the invoice as paid
        // This will trigger the MarkInvoiceAsPaidCommandHandler
        await messaging.InvokeCommandAsync(markPaidCommand, cancellationToken);
    }
}