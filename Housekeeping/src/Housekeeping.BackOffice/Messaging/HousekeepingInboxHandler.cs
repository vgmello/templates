// Copyright (c) ABCDEG. All rights reserved.

namespace Housekeeping.BackOffice.Messaging;

public static class HousekeepingInboxHandler
{
    public static class RoomStatusChangedHandler
    {
        public static async Task Handle(RoomStatusChanged @event, ILogger<RoomStatusChangedHandler> logger)
        {
            logger.LogInformation("Room {RoomNumber} status changed from {PreviousStatus} to {NewStatus}",
                @event.RoomNumber, @event.PreviousStatus, @event.NewStatus);

            // Add any business logic for room status changes
            // e.g., notifications, reporting, integration with other services
        }
    }

    public static class CleaningCompletedHandler
    {
        public static async Task Handle(CleaningCompleted @event, ILogger<CleaningCompletedHandler> logger)
        {
            logger.LogInformation("Cleaning completed for room {RoomNumber} by cleaner {CleanerId} in {Duration}",
                @event.RoomNumber, @event.CleanerId, @event.Duration);

            // Add any business logic for cleaning completion
            // e.g., cleaner performance tracking, invoicing
        }
    }

    public static class MaintenanceRequestedHandler
    {
        public static async Task Handle(MaintenanceRequested @event, ILogger<MaintenanceRequestedHandler> logger)
        {
            logger.LogInformation("Maintenance requested for room {RoomNumber}: {IssueType} (Priority: {Priority})",
                @event.RoomNumber, @event.IssueType, @event.Priority);

            // Add any business logic for maintenance requests
            // e.g., notifications to maintenance team, work order creation
        }
    }

    public static class MiniFridgeUpdatedHandler
    {
        public static async Task Handle(MiniFridgeUpdated @event, ILogger<MiniFridgeUpdatedHandler> logger)
        {
            logger.LogInformation("Mini fridge updated for room {RoomNumber} with total value {TotalValue:C}",
                @event.RoomNumber, @event.TotalValue);

            // Add any business logic for mini fridge updates
            // e.g., billing updates, inventory management
        }
    }
}