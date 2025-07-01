// Copyright (c) ABCDEG. All rights reserved.

using Housekeeping.Contracts.Rooms.Models;

namespace Housekeeping.BackOffice.Orleans.Rooms.Grains;

public sealed class RoomGrain([PersistentState("room")] IPersistentState<RoomState> state) : Grain, IRoomGrain
{
    private readonly IPersistentState<RoomState> _state = state;

    public Task<RoomStatus> GetStatusAsync()
    {
        return Task.FromResult(_state.State.Status);
    }

    public async Task SetStatusAsync(RoomStatus status, Guid? updatedBy = null)
    {
        _state.State.Status = status;
        _state.State.LastUpdatedBy = updatedBy;
        _state.State.LastUpdated = DateTimeOffset.UtcNow;
        
        await _state.WriteStateAsync();
    }

    public async Task StartCleaningAsync(Guid cleanerId)
    {
        _state.State.Status = RoomStatus.Cleaning;
        _state.State.AssignedCleanerId = cleanerId;
        _state.State.CleaningStarted = DateTimeOffset.UtcNow;
        _state.State.LastUpdated = DateTimeOffset.UtcNow;
        
        await _state.WriteStateAsync();
    }

    public async Task CompleteCleaningAsync(Guid cleanerId)
    {
        _state.State.Status = RoomStatus.Clean;
        _state.State.LastCleaned = DateTimeOffset.UtcNow;
        _state.State.CleaningCompleted = DateTimeOffset.UtcNow;
        _state.State.LastUpdated = DateTimeOffset.UtcNow;
        
        await _state.WriteStateAsync();
    }

    public async Task RequestMaintenanceAsync(string issueType, MaintenancePriority priority, Guid? reportedBy = null)
    {
        if (priority is MaintenancePriority.High or MaintenancePriority.Critical)
        {
            _state.State.Status = RoomStatus.Maintenance;
        }
        
        _state.State.LastMaintenanceRequest = DateTimeOffset.UtcNow;
        _state.State.LastUpdated = DateTimeOffset.UtcNow;
        
        await _state.WriteStateAsync();
    }

    public async Task<Dictionary<string, int>> UpdateMiniFridgeAsync(Dictionary<string, int> items, Guid? updatedBy = null)
    {
        _state.State.MiniFridgeItems = items;
        _state.State.LastUpdatedBy = updatedBy;
        _state.State.LastUpdated = DateTimeOffset.UtcNow;
        
        await _state.WriteStateAsync();
        return _state.State.MiniFridgeItems;
    }

    // Additional methods for Program.cs compatibility
    public Task<RoomState> GetState()
    {
        return Task.FromResult(_state.State);
    }

    public async Task UpdateStatus(string status)
    {
        if (Enum.TryParse<RoomStatus>(status, true, out var roomStatus))
        {
            _state.State.Status = roomStatus;
            _state.State.LastUpdated = DateTimeOffset.UtcNow;
            await _state.WriteStateAsync();
        }
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