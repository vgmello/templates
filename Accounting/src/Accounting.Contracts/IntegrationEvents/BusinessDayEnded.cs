// Copyright (c) ABCDEG. All rights reserved.

using System;

namespace Accounting.Contracts.IntegrationEvents;

/// <summary>
/// Integration event raised when a business day has ended.
/// This event is published to external services (e.g., via Kafka).
/// </summary>
public class BusinessDayEnded
{
    public DateTime BusinessDate { get; init; }
    public string MarketOrRegion { get; init; } = string.Empty;

    public BusinessDayEnded(DateTime businessDate, string marketOrRegion)
    {
        BusinessDate = businessDate;
        MarketOrRegion = marketOrRegion;
    }
}
