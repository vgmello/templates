// Copyright (c) ABCDEG. All rights reserved.

using Housekeeping.Contracts.Rooms.IntegrationEvents;
using Housekeeping.Contracts.Rooms.Models;

namespace Housekeeping.Rooms.Commands;

public record RecordCleaningCommand(
    Guid RoomId,
    Guid CleanerId,
    bool IsComplete,
    string? Notes = null) : ICommand<Result<Guid>>;

public class RecordCleaningValidator : AbstractValidator<RecordCleaningCommand>
{
    public RecordCleaningValidator()
    {
        RuleFor(c => c.RoomId).NotEmpty();
        RuleFor(c => c.CleanerId).NotEmpty();
        RuleFor(c => c.Notes).MaximumLength(500).When(c => c.Notes != null);
    }
}

public static partial class RecordCleaningCommandHandler
{
    [DbCommand(sp: "housekeeping.cleaning_record", nonQuery: true)]
    public partial record RecordCleaningDbCommand(
        Guid CleaningId,
        Guid RoomId,
        Guid CleanerId,
        bool IsComplete,
        string? Notes) : ICommand<int>;

    public static async Task<(Result<Guid>, CleaningCompleted?)> Handle(
        RecordCleaningCommand command,
        IMessageBus messaging,
        CancellationToken cancellationToken)
    {
        var cleaningId = Guid.CreateVersion7();

        var dbCommand = new RecordCleaningDbCommand(
            cleaningId,
            command.RoomId,
            command.CleanerId,
            command.IsComplete,
            command.Notes);

        await messaging.InvokeCommandAsync(dbCommand, cancellationToken);

        CleaningCompleted? completedEvent = null;

        if (command.IsComplete)
        {
            // In a real implementation, we'd fetch room number and start time from DB
            completedEvent = new CleaningCompleted(
                command.RoomId,
                "101", // Would come from DB
                command.CleanerId,
                DateTimeOffset.UtcNow.AddHours(-1), // Would come from DB
                DateTimeOffset.UtcNow,
                TimeSpan.FromHours(1));
        }

        return (cleaningId, completedEvent);
    }
}
