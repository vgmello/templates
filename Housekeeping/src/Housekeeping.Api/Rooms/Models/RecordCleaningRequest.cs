namespace Housekeeping.Api.Rooms;

public record RecordCleaningRequest(Guid CleanerId, bool IsComplete, string? Notes);