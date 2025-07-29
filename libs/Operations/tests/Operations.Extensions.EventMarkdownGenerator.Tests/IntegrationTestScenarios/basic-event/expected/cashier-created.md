---
editLink: false
---

# CashierCreated

- **Status:** Active
- **Version:** v1
- **Entity:** `cashier`
- **Type:** Integration Event
- **Topic:** `{env}.billing.public.cashiers.v1`
- **Partition Keys**: TenantId, PartitionKeyTest
## Description

Published when a new cashier is successfully created in the billing system

## When It's Triggered

This event is published when:
- The cashier creation process completes successfully

## Some other event data

Some other event data text

## Event Payload

| Property | Type | Required | Description |
| ----------------------------------------------------------------- | --------- | -------- | --------------------------------------------------------------------- |
| TenantId| `Guid` | ✓| Identifier of the tenant that owns the cashier (partition key) |
| PartitionKeyTest| `int` | ✓| Additional partition key for message routing (partition key) |
| [Cashier](./schemas/Billing.Cashiers.Contracts.IntegrationEvents.Cashier.md)| `Cashier` | ✓| Complete cashier object containing all cashier data and configuration |
### Partition Keys

This event uses multiple partition keys for message routing:
- `TenantId` - Primary partition key based on tenant
- `PartitionKeyTest` - Secondary partition key for routing optimization
### Reference Schemas

#### Cashier

<!--@include: ./schemas/Billing.Cashiers.Contracts.IntegrationEvents.Cashier.md#schema-->

## Technical Details

- **Full Type:** [Billing.Cashiers.Contracts.IntegrationEvents.CashierCreated](https://[github.url.from.config.com]/Billing/Cashiers/Contracts/IntegrationEvents/CashierCreated.cs)
- **Namespace:** `Billing.Cashiers.Contracts.IntegrationEvents`
- **Topic Attribute:** `[EventTopic<Cashier>]`