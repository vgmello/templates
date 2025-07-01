using Housekeeping.Contracts.Rooms.Models;

namespace Housekeeping.Api.Rooms;

public record UpdateRoomStatusRequest(RoomStatus Status, string? Notes, Guid? UpdatedBy);