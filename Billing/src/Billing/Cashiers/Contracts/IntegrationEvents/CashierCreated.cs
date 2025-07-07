// Copyright (c) ABCDEG. All rights reserved.

using Billing.Cashiers.Contracts.Models;

namespace Billing.Cashiers.Contracts.IntegrationEvents;

[EventTopic<Cashier>]
public record CashierCreated([PartitionKey] Guid TenantId, [PartitionKey] int PartitionKeyTest, Cashier Cashier);
