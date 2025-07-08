// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashiers.Contracts.Models;

namespace Billing.Cashiers.Contracts.IntegrationEvents;

[EventTopic<Cashier>]
public record CashierUpdated(Guid CashierId);
