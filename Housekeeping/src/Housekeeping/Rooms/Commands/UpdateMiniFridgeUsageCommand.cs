// Copyright (c) ABCDEG. All rights reserved.

using Housekeeping.Contracts.Rooms.IntegrationEvents;

namespace Housekeeping.Rooms.Commands;

public record UpdateMiniFridgeUsageCommand(
    Guid RoomId,
    Dictionary<string, int> Items,
    Guid? UpdatedBy = null) : ICommand<Result<Dictionary<string, int>>>;

public class UpdateMiniFridgeUsageValidator : AbstractValidator<UpdateMiniFridgeUsageCommand>
{
    public UpdateMiniFridgeUsageValidator()
    {
        RuleFor(c => c.RoomId).NotEmpty();
        RuleFor(c => c.Items).NotNull();
        RuleForEach(c => c.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Key).NotEmpty().MaximumLength(100);
            item.RuleFor(i => i.Value).GreaterThanOrEqualTo(0);
        });
    }
}

public static partial class UpdateMiniFridgeUsageCommandHandler
{
    [DbCommand(sp: "housekeeping.mini_fridge_update", nonQuery: true)]
    public partial record UpdateMiniFridgeDbCommand(
        Guid RoomId,
        string ItemsJson,
        Guid? UpdatedBy) : ICommand<int>;

    public static async Task<(Result<Dictionary<string, int>>, MiniFridgeUpdated)> Handle(
        UpdateMiniFridgeUsageCommand command,
        IMessageBus messaging,
        CancellationToken cancellationToken)
    {
        // Convert items to JSON for storage
        var itemsJson = System.Text.Json.JsonSerializer.Serialize(command.Items);
        
        var dbCommand = new UpdateMiniFridgeDbCommand(
            command.RoomId,
            itemsJson,
            command.UpdatedBy);

        await messaging.InvokeCommandAsync(dbCommand, cancellationToken);

        // Calculate total value (simplified - in real app would use pricing service)
        var totalValue = command.Items.Sum(i => i.Value * 5.00m);

        var updatedEvent = new MiniFridgeUpdated(
            command.RoomId,
            "101", // Would fetch from DB
            command.Items,
            totalValue,
            command.UpdatedBy,
            DateTimeOffset.UtcNow);

        return (command.Items, updatedEvent);
    }
}