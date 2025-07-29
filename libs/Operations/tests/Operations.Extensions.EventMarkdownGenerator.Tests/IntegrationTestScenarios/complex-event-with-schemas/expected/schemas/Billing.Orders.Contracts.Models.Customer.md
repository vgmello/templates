# Customer

Represents customer information for order processing.

## Properties

| Property | Type | Required | Description |
| ----------------------------------------------------------------- | --------- | -------- | --------------------------------------------------------------------- |
| CustomerId | `Guid` | ✓ | Unique identifier for the customer |
| Name | `string` | ✓ | Full name of the customer |
| Email | `string` | ✓ | Email address for order notifications |
| [BillingAddress](./Billing.Orders.Contracts.Models.Address.md) | `Address` | ✓ | Complete billing address information |