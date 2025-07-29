---
editLink: false
---

# InvoiceFinalized

- **Status:** Active
- **Version:** v1
- **Entity:** `invoice`
- **Type:** Integration Event
- **Topic:** `{env}.billing.external.invoices.v1`
- **Estimated Payload Size:** 68 bytes
- **Partition Keys**: TenantId
## Description

No documentation available

## Event Payload

| Property | Type | Required | Size | Description |
| ----------------------------------------------------------------- | --------- | -------- | -------- | --------------------------------------------------------------------- |
| TenantId| `Guid` | ✓| 16 bytes | No description available (partition key) |
| InvoiceId| `Guid` | ✓| 16 bytes | No description available |
| CustomerId| `Guid` | ✓| 16 bytes | No description available |
| PublicInvoiceNumber| `string` | ✓| 4 bytes | No description available |
| FinalTotalAmount| `decimal` | ✓| 16 bytes | No description available |


### Partition Keys

This event uses a partition key for message routing:
- `TenantId` - No description available
## Technical Details

- **Full Type:** [Billing.Invoices.Contracts.IntegrationEvents.InvoiceFinalized](https://github.com/vgmello/templates/blob/main/src/Billing/Invoices/Contracts/IntegrationEvents/InvoiceFinalized.cs)
- **Namespace:** `Billing.Invoices.Contracts.IntegrationEvents`
- **Topic Attribute:** `[EventTopic]`
