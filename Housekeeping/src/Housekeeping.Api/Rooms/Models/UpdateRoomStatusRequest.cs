// Copyright (c) ABCDEG. All rights reserved.

using Housekeeping.Contracts.Rooms.Models;

namespace Housekeeping.Api.Rooms.Models;

public record UpdateRoomStatusRequest(RoomStatus Status, string? Notes, Guid? UpdatedBy);
