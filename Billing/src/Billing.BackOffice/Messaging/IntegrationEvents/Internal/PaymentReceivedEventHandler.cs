// Copyright (c) ABCDEG. All rights reserved.

using Billing.Contracts.Invoices.IntegrationEvents;
using Billing.Invoices.Commands;

namespace Billing.BackOffice.Messaging.IntegrationEvents.Internal;

public static class PaymentReceivedEventHandler
{
    public static async Task Handle(PaymentReceivedEvent @event, IMessageBus messaging, CancellationToken cancellationToken)
    {
        var markPaidCommand = new MarkInvoiceAsPaidCommand(
            @event.InvoiceId,
            @event.Amount,
            @event.ReceivedDate
        );

        await messaging.InvokeCommandAsync(markPaidCommand, cancellationToken);
    }
}
