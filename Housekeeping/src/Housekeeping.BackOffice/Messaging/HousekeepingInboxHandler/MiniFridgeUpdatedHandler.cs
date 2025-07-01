// Copyright (c) ABCDEG. All rights reserved.

using Housekeeping.Contracts.Rooms.IntegrationEvents;

namespace Housekeeping.BackOffice.Messaging.HousekeepingInboxHandler;

public static class MiniFridgeUpdatedHandler
{
    public static async Task Handle(MiniFridgeUpdated @event, CancellationToken cancellationToken)
    {
        // Add any business logic for mini fridge updates
        // e.g., billing updates, inventory management
        
        await Task.CompletedTask;
    }
}