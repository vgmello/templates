// Copyright (c) ABCDEG. All rights reserved.

using Housekeeping.Contracts.Rooms.IntegrationEvents;

namespace Housekeeping.BackOffice.Messaging.HousekeepingInboxHandler;

public static class CleaningCompletedHandler
{
    public static async Task Handle(CleaningCompleted @event, CancellationToken cancellationToken)
    {
        // Add any business logic for cleaning completion
        // e.g., cleaner performance tracking, invoicing
        
        await Task.CompletedTask;
    }
}