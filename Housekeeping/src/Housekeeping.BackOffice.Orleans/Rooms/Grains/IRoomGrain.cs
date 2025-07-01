// Copyright (c) ABCDEG. All rights reserved.

using Housekeeping.Contracts.Rooms.Models;

namespace Housekeeping.BackOffice.Orleans.Rooms.Grains;

public interface IRoomGrain : IGrainWithIntegerKey
{
    Task<RoomStatus> GetStatusAsync();
    Task SetStatusAsync(RoomStatus status, Guid? updatedBy = null);
    Task StartCleaningAsync(Guid cleanerId);
    Task CompleteCleaningAsync(Guid cleanerId);
    Task RequestMaintenanceAsync(string issueType, MaintenancePriority priority, Guid? reportedBy = null);
    Task<Dictionary<string, int>> UpdateMiniFridgeAsync(Dictionary<string, int> items, Guid? updatedBy = null);
    
    // Additional methods for Program.cs compatibility
    Task<RoomState> GetState();
    Task UpdateStatus(string status);
}