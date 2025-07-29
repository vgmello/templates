---
editLink: false
---

# InvoicePaid

- **Status:** Active
- **Version:** v1
- **Entity:** `invoice`
- **Type:** Integration Event
- **Topic:** `{env}.billing.external.invoices.v1`
- **Estimated Payload Size:** 132 bytes ⚠️ *Contains dynamic properties*
- **Partition Keys**: TenantId
## Description

No documentation available

## Event Payload

| Property | Type | Required | Size | Description |
| ----------------------------------------------------------------- | --------- | -------- | -------- | --------------------------------------------------------------------- |
| TenantId| `Guid` | ✓| 16 bytes | No description available (partition key) |
| [Invoice](./schemas/Billing.Invoices.Contracts.Models.Invoice.md)| `Invoice` | ✓| 116 bytes (Name: Dynamic size - no MaxLength constraint, Status: Dynamic size - no MaxLength constraint, Currency: Dynamic size - no MaxLength constraint) | No description available |

### Partition Keys

This event uses a partition key for message routing:
- `TenantId` - Primary partition key based on tenant

### Reference Schemas

#### Invoice

<!--@include: ./schemas/Billing.Invoices.Contracts.Models.Invoice.md#schema-->

## Technical Details

- **Full Type:** [Billing.Invoices.Contracts.IntegrationEvents.InvoicePaid](https://[github.url.from.config.com]/Billing/Invoices/Contracts/IntegrationEvents/InvoicePaid.cs)
- **Namespace:** `Billing.Invoices.Contracts.IntegrationEvents`
- **Topic Attribute:** `[EventTopic]`
