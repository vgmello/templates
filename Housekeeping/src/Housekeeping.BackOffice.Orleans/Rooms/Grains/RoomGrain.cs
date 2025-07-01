// Copyright (c) ABCDEG. All rights reserved.

using Housekeeping.Contracts.Rooms.Models;

namespace Housekeeping.BackOffice.Orleans.Rooms.Grains;

public class RoomGrain : Grain, IRoomGrain
{
    private RoomState _state = new();

    public Task<RoomStatus> GetStatusAsync()
    {
        return Task.FromResult(_state.Status);
    }

    public Task SetStatusAsync(RoomStatus status, Guid? updatedBy = null)
    {
        _state.Status = status;
        _state.LastUpdatedBy = updatedBy;
        _state.LastUpdated = DateTimeOffset.UtcNow;
        
        return Task.CompletedTask;
    }

    public Task StartCleaningAsync(Guid cleanerId)
    {
        _state.Status = RoomStatus.Cleaning;
        _state.AssignedCleanerId = cleanerId;
        _state.CleaningStarted = DateTimeOffset.UtcNow;
        _state.LastUpdated = DateTimeOffset.UtcNow;
        
        return Task.CompletedTask;
    }

    public Task CompleteCleaningAsync(Guid cleanerId)
    {
        _state.Status = RoomStatus.Clean;
        _state.LastCleaned = DateTimeOffset.UtcNow;
        _state.CleaningCompleted = DateTimeOffset.UtcNow;
        _state.LastUpdated = DateTimeOffset.UtcNow;
        
        return Task.CompletedTask;
    }

    public Task RequestMaintenanceAsync(string issueType, MaintenancePriority priority, Guid? reportedBy = null)
    {
        if (priority is MaintenancePriority.High or MaintenancePriority.Critical)
        {
            _state.Status = RoomStatus.Maintenance;
        }
        
        _state.LastMaintenanceRequest = DateTimeOffset.UtcNow;
        _state.LastUpdated = DateTimeOffset.UtcNow;
        
        return Task.CompletedTask;
    }

    public Task<Dictionary<string, int>> UpdateMiniFridgeAsync(Dictionary<string, int> items, Guid? updatedBy = null)
    {
        _state.MiniFridgeItems = items;
        _state.LastUpdatedBy = updatedBy;
        _state.LastUpdated = DateTimeOffset.UtcNow;
        
        return Task.FromResult(_state.MiniFridgeItems);
    }
}

public class RoomState
{
    public RoomStatus Status { get; set; } = RoomStatus.Clean;
    public Guid? AssignedCleanerId { get; set; }
    public DateTimeOffset? LastCleaned { get; set; }
    public DateTimeOffset? CleaningStarted { get; set; }
    public DateTimeOffset? CleaningCompleted { get; set; }
    public DateTimeOffset? LastMaintenanceRequest { get; set; }
    public Dictionary<string, int> MiniFridgeItems { get; set; } = new();
    public Guid? LastUpdatedBy { get; set; }
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}