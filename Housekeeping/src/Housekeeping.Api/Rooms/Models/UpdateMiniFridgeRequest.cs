namespace Housekeeping.Api.Rooms;

public record UpdateMiniFridgeRequest(Dictionary<string, int> Items, Guid? UpdatedBy);