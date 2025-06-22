// Copyright (c) ABCDEG. All rights reserved.

namespace Accounting.Contracts.Operations.IntegrationEvents;

public class BusinessDayEnded
{
    public string TenantId { get; set; } = string.Empty;
    public DateOnly BusinessDate { get; set; }
    public string Market { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
}
