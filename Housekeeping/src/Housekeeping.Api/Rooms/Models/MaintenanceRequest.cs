using Housekeeping.Contracts.Rooms.Models;

namespace Housekeeping.Api.Rooms;

public record MaintenanceRequest(string IssueType, string? Description, MaintenancePriority Priority, Guid? ReportedBy);