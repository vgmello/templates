// Copyright (c) ABCDEG. All rights reserved.

using Housekeeping.Contracts.Rooms.Models;

namespace Housekeeping.Contracts.Rooms.IntegrationEvents;

/// <summary>
///     Published when a room's status changes
/// </summary>
public record RoomStatusChanged(
    Guid RoomId,
    string RoomNumber,
    RoomStatus PreviousStatus,
    RoomStatus NewStatus,
    Guid? ChangedBy,
    DateTimeOffset ChangedDateUtc);