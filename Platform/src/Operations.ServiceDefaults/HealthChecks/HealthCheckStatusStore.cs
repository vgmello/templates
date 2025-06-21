// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Operations.ServiceDefaults.HealthChecks;

public class HealthCheckStatusStore
{
    public HealthStatus LastHealthStatus { get; set; } = HealthStatus.Healthy;
}
