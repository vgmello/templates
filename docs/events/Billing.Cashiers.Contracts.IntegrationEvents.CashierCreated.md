---
editLink: false
---

# CashierCreated

- **Status:** Active
- **Version:** v1
- **Entity:** `cashier`
- **Type:** Integration Event
- **Topic:** `{env}.billing.external.cashiers.v1`
- **Estimated Payload Size:** 224 bytes ⚠️ *Contains dynamic properties*
- **Partition Keys**: TenantId, PartitionKeyTest
## Description

Published when a new cashier is successfully created in the billing system. This event contains the complete cashier data and partition
key information for proper message routing.

## When It's Triggered

This event is published when:
-   The cashier creation process completes successfully

## Some other event data

Some other event data text
## Event Payload

| Property | Type | Required | Size | Description |
| ----------------------------------------------------------------- | --------- | -------- | -------- | --------------------------------------------------------------------- |
| TenantId| `Guid` | ✓| 16 bytes | Unique identifier for the tenant (partition key) |
| PartitionKeyTest| `int` | ✓| 4 bytes | Additional partition key for message routing (partition key) |
| [Cashier](/events/schemas/Billing.Cashiers.Contracts.Models.Cashier.md)| `Cashier` | ✓| 204 bytes (CashierPayments: Collection size estimated (no Range constraint)) | Complete cashier object containing all cashier data and configuration |


### Partition Keys

This event uses multiple partition keys for message routing:
- `TenantId` - Unique identifier for the tenant
- `PartitionKeyTest` - Additional partition key for message routing

### Reference Schemas

#### Cashier

<!--@include: @/events/schemas/Billing.Cashiers.Contracts.Models.Cashier.md#schema-->

## Technical Details

- **Full Type:** [Billing.Cashiers.Contracts.IntegrationEvents.CashierCreated](https://github.com/vgmello/templates/blob/main/src/Billing/Cashiers/Contracts/IntegrationEvents/CashierCreated.cs)
- **Namespace:** `Billing.Cashiers.Contracts.IntegrationEvents`
- **Topic Attribute:** `[EventTopic]`
