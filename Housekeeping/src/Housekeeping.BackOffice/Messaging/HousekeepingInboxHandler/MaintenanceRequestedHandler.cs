// Copyright (c) ABCDEG. All rights reserved.

using Housekeeping.Contracts.Rooms.IntegrationEvents;

namespace Housekeeping.BackOffice.Messaging.HousekeepingInboxHandler;

public static class MaintenanceRequestedHandler
{
    public static async Task Handle(MaintenanceRequested @event, CancellationToken cancellationToken)
    {
        // Add any business logic for maintenance requests
        // e.g., notifications to maintenance team, work order creation
        
        await Task.CompletedTask;
    }
}