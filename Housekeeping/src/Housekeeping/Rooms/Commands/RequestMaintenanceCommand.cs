// Copyright (c) ABCDEG. All rights reserved.

using Housekeeping.Contracts.Rooms.IntegrationEvents;
using Housekeeping.Contracts.Rooms.Models;

namespace Housekeeping.Rooms.Commands;

public record RequestMaintenanceCommand(
    Guid RoomId,
    string IssueType,
    string? Description,
    MaintenancePriority Priority,
    Guid? ReportedBy = null) : ICommand<Result<Guid>>;

public class RequestMaintenanceValidator : AbstractValidator<RequestMaintenanceCommand>
{
    public RequestMaintenanceValidator()
    {
        RuleFor(c => c.RoomId).NotEmpty();
        RuleFor(c => c.IssueType).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Description).MaximumLength(500).When(c => c.Description != null);
        RuleFor(c => c.Priority).IsInEnum();
    }
}

public static partial class RequestMaintenanceCommandHandler
{
    [DbCommand(sp: "housekeeping.maintenance_request_create", nonQuery: true)]
    public partial record CreateMaintenanceRequestDbCommand(
        Guid RequestId,
        Guid RoomId,
        string IssueType,
        string? Description,
        string Priority,
        Guid? ReportedBy) : ICommand<int>;

    public static async Task<(Result<Guid>, MaintenanceRequested)> Handle(
        RequestMaintenanceCommand command,
        IMessageBus messaging,
        CancellationToken cancellationToken)
    {
        var requestId = Guid.CreateVersion7();
        
        var dbCommand = new CreateMaintenanceRequestDbCommand(
            requestId,
            command.RoomId,
            command.IssueType,
            command.Description,
            command.Priority.ToString(),
            command.ReportedBy);

        await messaging.InvokeCommandAsync(dbCommand, cancellationToken);

        var requestedEvent = new MaintenanceRequested(
            requestId,
            command.RoomId,
            "101", // Would fetch from DB
            command.IssueType,
            command.Description,
            command.Priority,
            command.ReportedBy,
            DateTimeOffset.UtcNow);

        return (requestId, requestedEvent);
    }
}