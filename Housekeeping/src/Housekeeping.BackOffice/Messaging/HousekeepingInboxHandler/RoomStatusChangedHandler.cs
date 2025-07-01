// Copyright (c) ABCDEG. All rights reserved.

using Housekeeping.Contracts.Rooms.IntegrationEvents;

namespace Housekeeping.BackOffice.Messaging.HousekeepingInboxHandler;

public static class RoomStatusChangedHandler
{
    public static async Task Handle(RoomStatusChanged @event, CancellationToken cancellationToken)
    {
        // Add any business logic for room status changes
        // e.g., notifications, reporting, integration with other services
        
        await Task.CompletedTask;
    }
}