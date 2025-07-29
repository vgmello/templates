---
editLink: false
---

# InvoiceFinalized

- **Status:** Active
- **Version:** v1
- **Entity:** `invoice`
- **Type:** Integration Event
- **Topic:** `{env}.billing.external.invoices.v1`
- **Estimated Payload Size:** 64 bytes ⚠️ *Contains dynamic properties*
- **Partition Keys**: TenantId
## Description

No documentation available

## Event Payload

| Property | Type | Required | Size | Description |
| ----------------------------------------------------------------- | --------- | -------- | -------- | --------------------------------------------------------------------- |
| TenantId| `Guid` | ✓| 16 bytes | No description available (partition key) |
| InvoiceId| `Guid` | ✓| 16 bytes | No description available |
| CustomerId| `Guid` | ✓| 16 bytes | No description available |
| PublicInvoiceNumber| `string` | ✓| 0 bytes (Dynamic size - no MaxLength constraint) | No description available |
| FinalTotalAmount| `decimal` | ✓| 16 bytes | No description available |

### Partition Keys

This event uses a partition key for message routing:
- `TenantId` - Primary partition key based on tenant
## Technical Details

- **Full Type:** [Billing.Invoices.Contracts.IntegrationEvents.InvoiceFinalized](https://[github.url.from.config.com]/Billing/Invoices/Contracts/IntegrationEvents/InvoiceFinalized.cs)
- **Namespace:** `Billing.Invoices.Contracts.IntegrationEvents`
- **Topic Attribute:** `[EventTopic]`
