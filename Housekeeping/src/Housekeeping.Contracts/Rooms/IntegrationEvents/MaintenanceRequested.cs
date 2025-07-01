// Copyright (c) ABCDEG. All rights reserved.

using Housekeeping.Contracts.Rooms.Models;

namespace Housekeeping.Contracts.Rooms.IntegrationEvents;

/// <summary>
///     Published when maintenance is requested for a room
/// </summary>
public record MaintenanceRequested(
    Guid RequestId,
    Guid RoomId,
    string RoomNumber,
    string IssueType,
    string? Description,
    MaintenancePriority Priority,
    Guid? ReportedBy,
    DateTimeOffset RequestedDateUtc);