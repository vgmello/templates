// Copyright (c) ABCDEG. All rights reserved.

using Housekeeping.Contracts.Rooms.Models;

namespace Housekeeping.Rooms.Queries;

public record GetRoomsStatusQuery(
    RoomStatus? Status = null,
    int? Floor = null,
    Guid? AssignedCleanerId = null,
    int PageNumber = 1,
    int PageSize = 50) : IQuery<IEnumerable<GetRoomsStatusQuery.Result>>
{
    public record Result(
        Guid RoomId,
        string RoomNumber,
        int Floor,
        RoomStatus Status,
        DateTimeOffset? LastCleanedDateUtc,
        Guid? AssignedCleanerId,
        Dictionary<string, int> MiniFridgeItems,
        string? Notes);
}

public static partial class GetRoomsStatusQueryHandler
{
    [DbCommand(sp: "housekeeping.rooms_get")]
    public partial record GetRoomsDbQuery(
        string? Status,
        int? Floor,
        Guid? AssignedCleanerId,
        int PageNumber,
        int PageSize) : IQuery<IEnumerable<GetRoomsStatusQuery.Result>>;

    public static async Task<IEnumerable<GetRoomsStatusQuery.Result>> Handle(
        GetRoomsStatusQuery query,
        IMessageBus messaging,
        CancellationToken cancellationToken)
    {
        var dbQuery = new GetRoomsDbQuery(
            query.Status?.ToString(),
            query.Floor,
            query.AssignedCleanerId,
            query.PageNumber,
            query.PageSize);

        return await messaging.InvokeQueryAsync(dbQuery, cancellationToken);
    }
}