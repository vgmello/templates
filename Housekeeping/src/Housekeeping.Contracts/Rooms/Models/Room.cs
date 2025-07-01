// Copyright (c) ABCDEG. All rights reserved.

namespace Housekeeping.Contracts.Rooms.Models;

/// <summary>
///     Represents a room's housekeeping status and information
/// </summary>
public class Room
{
    /// <summary>
    ///     Unique identifier for the room
    /// </summary>
    public Guid RoomId { get; init; }
    
    /// <summary>
    ///     Room number (e.g., "101", "202A")
    /// </summary>
    public string RoomNumber { get; init; } = string.Empty;
    
    /// <summary>
    ///     Floor number where the room is located
    /// </summary>
    public int Floor { get; init; }
    
    /// <summary>
    ///     Current cleanliness/maintenance status
    /// </summary>
    public RoomStatus Status { get; init; }
    
    /// <summary>
    ///     Date and time when room was last cleaned
    /// </summary>
    public DateTimeOffset? LastCleanedDateUtc { get; init; }
    
    /// <summary>
    ///     ID of the cleaner assigned to this room
    /// </summary>
    public Guid? AssignedCleanerId { get; init; }
    
    /// <summary>
    ///     Mini fridge inventory/usage information
    /// </summary>
    public Dictionary<string, int> MiniFridgeItems { get; init; } = new();
    
    /// <summary>
    ///     Additional notes about the room
    /// </summary>
    public string? Notes { get; init; }
    
    public DateTimeOffset CreatedDateUtc { get; init; }
    public DateTimeOffset UpdatedDateUtc { get; init; }
    public int Version { get; init; }
}