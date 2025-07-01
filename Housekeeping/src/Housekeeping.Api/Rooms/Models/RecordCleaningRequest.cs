// Copyright (c) ABCDEG. All rights reserved.

namespace Housekeeping.Api.Rooms.Models;

public record RecordCleaningRequest(Guid CleanerId, bool IsComplete, string? Notes);
