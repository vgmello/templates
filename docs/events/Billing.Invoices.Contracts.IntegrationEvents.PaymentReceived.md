---
editLink: false
---

# PaymentReceived

- **Status:** Active
- **Version:** v1
- **Entity:** ``
- **Type:** Integration Event
- **Topic:** `{env}.billing.external.payments.v1`
- **Estimated Payload Size:** 68 bytes
- **Partition Keys**: TenantId
## Description

No documentation available

## Event Payload

| Property | Type | Required | Size | Description |
| ----------------------------------------------------------------- | --------- | -------- | -------- | --------------------------------------------------------------------- |
| TenantId| `Guid` | ✓| 16 bytes | No description available (partition key) |
| InvoiceId| `Guid` | ✓| 16 bytes | No description available |
| Amount| `decimal` | ✓| 16 bytes | No description available |
| Currency| `string` | ✓| 4 bytes | No description available |
| PaymentMethod| `string` | ✓| 4 bytes | No description available |
| PaymentReference| `string` | ✓| 4 bytes | No description available |
| ReceivedDate| `DateTime` | ✓| 8 bytes | No description available |


### Partition Keys

This event uses a partition key for message routing:
- `TenantId` - No description available
## Technical Details

- **Full Type:** [Billing.Invoices.Contracts.IntegrationEvents.PaymentReceived](https://github.com/vgmello/templates/blob/main/src/Billing/Invoices/Contracts/IntegrationEvents/PaymentReceived.cs)
- **Namespace:** `Billing.Invoices.Contracts.IntegrationEvents`
- **Topic Attribute:** `[EventTopic]`
