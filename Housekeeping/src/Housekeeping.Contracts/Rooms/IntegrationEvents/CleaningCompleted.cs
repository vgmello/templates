// Copyright (c) ABCDEG. All rights reserved.

namespace Housekeeping.Contracts.Rooms.IntegrationEvents;

/// <summary>
///     Published when room cleaning is completed
/// </summary>
public record CleaningCompleted(
    Guid RoomId,
    string RoomNumber,
    Guid CleanerId,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    TimeSpan Duration);
