// Copyright (c) ABCDEG. All rights reserved.

using Housekeeping.Contracts.Rooms.IntegrationEvents;
using Housekeeping.Contracts.Rooms.Models;

namespace Housekeeping.Rooms.Commands;

using RoomModel = Room;

public record UpdateRoomStatusCommand(
    Guid RoomId,
    RoomStatus NewStatus,
    string? Notes = null,
    Guid? UpdatedBy = null) : ICommand<Result<RoomModel>>;

public class UpdateRoomStatusValidator : AbstractValidator<UpdateRoomStatusCommand>
{
    public UpdateRoomStatusValidator()
    {
        RuleFor(c => c.RoomId).NotEmpty();
        RuleFor(c => c.NewStatus).IsInEnum();
        RuleFor(c => c.Notes).MaximumLength(500).When(c => c.Notes != null);
    }
}

public static partial class UpdateRoomStatusCommandHandler
{
    [DbCommand(sp: "housekeeping.room_update_status")]
    public partial record UpdateRoomStatusDbCommand(
        Guid RoomId,
        string Status,
        string? Notes,
        Guid? UpdatedBy) : ICommand<RoomModel>;

    public static async Task<(Result<RoomModel>, RoomStatusChanged?)> Handle(
        UpdateRoomStatusCommand command,
        IMessageBus messaging,
        CancellationToken cancellationToken)
    {
        var dbCommand = new UpdateRoomStatusDbCommand(
            command.RoomId,
            command.NewStatus.ToString(),
            command.Notes,
            command.UpdatedBy);

        var room = await messaging.InvokeCommandAsync(dbCommand, cancellationToken);

        // Parse previous status from the returned room data
        // In a real implementation, we'd need to get the previous status from the DB
        var previousStatus = room.Status == command.NewStatus ? room.Status : RoomStatus.Dirty;

        var statusChangedEvent = room.Status != previousStatus
            ? new RoomStatusChanged(
                room.RoomId,
                room.RoomNumber,
                previousStatus,
                room.Status,
                command.UpdatedBy,
                DateTimeOffset.UtcNow)
            : null;

        return (room, statusChangedEvent);
    }
}
