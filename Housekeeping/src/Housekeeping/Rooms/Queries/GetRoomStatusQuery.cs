// Copyright (c) ABCDEG. All rights reserved.

using Housekeeping.Contracts.Rooms.Models;

namespace Housekeeping.Rooms.Queries;

using RoomModel = Room;

public record GetRoomStatusQuery(Guid RoomId) : IQuery<RoomModel>;

public static partial class GetRoomStatusQueryHandler
{
    [DbCommand(sp: "housekeeping.room_get")]
    public partial record GetRoomDbQuery(Guid RoomId) : IQuery<RoomModel>;

    public static async Task<RoomModel> Handle(GetRoomStatusQuery query, IMessageBus messaging, CancellationToken cancellationToken)
    {
        var dbQuery = new GetRoomDbQuery(query.RoomId);

        return await messaging.InvokeQueryAsync(dbQuery, cancellationToken);
    }
}
