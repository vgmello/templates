// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashiers.Contracts.IntegrationEvents;

namespace Billing.BackOffice.Messaging.BillingInboxHandler;

public static class CashierCreatedHandler
{
    public static Task Handle(CashierCreated @event, ILogger logger, IMessageBus messaging, CancellationToken cancellationToken)
    {
        logger.LogDebug("Nice !!, It works. CashierCreated received by the backoffice {@Cashier}", @event.Cashier);

        return Task.CompletedTask;
    }
}
