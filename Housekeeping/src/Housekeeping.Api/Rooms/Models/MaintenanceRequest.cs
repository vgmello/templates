// Copyright (c) ABCDEG. All rights reserved.

using Housekeeping.Contracts.Rooms.Models;

namespace Housekeeping.Api.Rooms.Models;

public record MaintenanceRequest(string IssueType, string? Description, MaintenancePriority Priority, Guid? ReportedBy);
