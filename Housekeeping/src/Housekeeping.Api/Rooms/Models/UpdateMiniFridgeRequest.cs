// Copyright (c) ABCDEG. All rights reserved.

namespace Housekeeping.Api.Rooms.Models;

public record UpdateMiniFridgeRequest(Dictionary<string, int> Items, Guid? UpdatedBy);
