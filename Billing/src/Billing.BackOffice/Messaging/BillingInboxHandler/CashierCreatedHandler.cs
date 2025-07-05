// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashiers.Contracts.IntegrationEvents;

namespace Billing.BackOffice.Messaging.BillingInboxHandler;

public static class CashierCreatedHandler
{
    public static Task Handle(CashierCreated @event, ILogger logger, IMessageBus messaging, CancellationToken cancellationToken)
    {
        logger.LogDebug("Event received. {@Cashier}", @event.Cashier);

        return Task.CompletedTask;
    }
}
