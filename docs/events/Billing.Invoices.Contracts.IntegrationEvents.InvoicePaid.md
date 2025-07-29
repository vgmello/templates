---
editLink: false
---

# InvoicePaid

- **Status:** Active
- **Version:** v1
- **Entity:** `invoice`
- **Type:** Integration Event
- **Topic:** `{env}.billing.external.invoices.v1`
- **Estimated Payload Size:** 144 bytes
- **Partition Keys**: TenantId
## Description

No documentation available

## Event Payload

| Property | Type | Required | Size | Description |
| ----------------------------------------------------------------- | --------- | -------- | -------- | --------------------------------------------------------------------- |
| TenantId| `Guid` | ✓| 16 bytes | No description available (partition key) |
| [Invoice](/events/schemas/Billing.Invoices.Contracts.Models.Invoice.md)| `Invoice` | ✓| 128 bytes | No description available |


### Partition Keys

This event uses a partition key for message routing:
- `TenantId` - No description available

### Reference Schemas

#### Invoice

<!--@include: @/events/schemas/Billing.Invoices.Contracts.Models.Invoice.md#schema-->

## Technical Details

- **Full Type:** [Billing.Invoices.Contracts.IntegrationEvents.InvoicePaid](https://github.com/vgmello/templates/blob/main/src/Billing/Invoices/Contracts/IntegrationEvents/InvoicePaid.cs)
- **Namespace:** `Billing.Invoices.Contracts.IntegrationEvents`
- **Topic Attribute:** `[EventTopic]`
