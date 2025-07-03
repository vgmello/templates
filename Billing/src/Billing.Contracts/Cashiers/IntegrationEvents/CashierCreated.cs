// Copyright (c) ABCDEG. All rights reserved.

using Billing.Contracts.Cashiers.Models;
using Operations.Extensions.Abstractions.Messaging;

namespace Billing.Contracts.Cashiers.IntegrationEvents;

[EventTopic<Cashier>]
public record CashierCreated([property: PartitionKey] Guid TenantId, Cashier Cashier);
