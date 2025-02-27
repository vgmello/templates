// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Operations.ServiceDefaults.HealthChecks;

public class HealthCheckStatusStore
{
    private HealthStatus _lastStatus = HealthStatus.Healthy;

    public void StoreHealthStatus(HealthReport report) => _lastStatus = report.Status;

    public HealthStatus GetLastStatus() => _lastStatus;
}
