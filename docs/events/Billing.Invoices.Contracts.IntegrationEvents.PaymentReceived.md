---
editLink: false
---

# PaymentReceived

- **Status:** Active
- **Version:** v1
- **Entity:** ``
- **Type:** Integration Event
- **Topic:** `{env}.billing.external.payments.v1`
- **Estimated Payload Size:** 56 bytes ⚠️ *Contains dynamic properties*
- **Partition Keys**: TenantId
## Description

No documentation available

## Event Payload

| Property | Type | Required | Size | Description |
| ----------------------------------------------------------------- | --------- | -------- | -------- | --------------------------------------------------------------------- |
| TenantId| `Guid` | ✓| 16 bytes | No description available (partition key) |
| InvoiceId| `Guid` | ✓| 16 bytes | No description available |
| Amount| `decimal` | ✓| 16 bytes | No description available |
| Currency| `string` | ✓| 0 bytes (Dynamic size - no MaxLength constraint) | No description available |
| PaymentMethod| `string` | ✓| 0 bytes (Dynamic size - no MaxLength constraint) | No description available |
| PaymentReference| `string` | ✓| 0 bytes (Dynamic size - no MaxLength constraint) | No description available |
| ReceivedDate| `DateTime` | ✓| 8 bytes | No description available |

### Partition Keys

This event uses a partition key for message routing:
- `TenantId` - Primary partition key based on tenant
## Technical Details

- **Full Type:** [Billing.Invoices.Contracts.IntegrationEvents.PaymentReceived](https://[github.url.from.config.com]/Billing/Invoices/Contracts/IntegrationEvents/PaymentReceived.cs)
- **Namespace:** `Billing.Invoices.Contracts.IntegrationEvents`
- **Topic Attribute:** `[EventTopic]`
