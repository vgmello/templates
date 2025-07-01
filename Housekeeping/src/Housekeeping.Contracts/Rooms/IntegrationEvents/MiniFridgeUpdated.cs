// Copyright (c) ABCDEG. All rights reserved.

namespace Housekeeping.Contracts.Rooms.IntegrationEvents;

/// <summary>
///     Published when mini fridge items are updated
/// </summary>
public record MiniFridgeUpdated(
    Guid RoomId,
    string RoomNumber,
    Dictionary<string, int> Items,
    decimal TotalValue,
    Guid? UpdatedBy,
    DateTimeOffset UpdatedDateUtc);
