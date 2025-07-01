// Copyright (c) ABCDEG. All rights reserved.

namespace Housekeeping.Contracts.Rooms.Models;

/// <summary>
///     Represents the cleanliness and maintenance status of a room
/// </summary>
public enum RoomStatus
{
    /// <summary>
    ///     Room is clean and ready for guests
    /// </summary>
    Clean,
    
    /// <summary>
    ///     Room requires cleaning
    /// </summary>
    Dirty,
    
    /// <summary>
    ///     Room is currently being cleaned
    /// </summary>
    Cleaning,
    
    /// <summary>
    ///     Room has been cleaned and inspected
    /// </summary>
    Inspected,
    
    /// <summary>
    ///     Room is under maintenance
    /// </summary>
    Maintenance,
    
    /// <summary>
    ///     Room is temporarily out of service
    /// </summary>
    OutOfService
}