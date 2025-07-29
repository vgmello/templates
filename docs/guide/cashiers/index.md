# Cashiers Guide

The Cashiers domain manages the personnel responsible for handling payments and billing operations within the Billing Solution. This guide covers everything you need to know about working with cashiers.

## Overview

Cashiers are the primary actors in the billing system who:

-   Process invoice payments
-   Manage customer transactions
-   Handle payment reconciliation
-   Generate payment reports

The Cashier model represents these real-world billing personnel digitally, tracking their information and payment history.

## Domain Model

The Cashier entity is defined in [`src/Billing/Cashiers/Contracts/Models/Cashier.cs:5-16`](https://github.com/yourusername/billing/blob/main/src/Billing/Cashiers/Contracts/Models/Cashier.cs#L5-L16):

<<< @/../src/Billing/Cashiers/Contracts/Models/Cashier.cs

## API Operations

### Create a Cashier

**Endpoint**: `POST /api/cashiers`

Creates a new cashier in the system.

```bash
curl -X POST http://localhost:8101/api/cashiers \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Jane Doe",
    "email": "jane.doe@billing.com"
  }'
```

**Command**: [`src/Billing/Cashiers/Commands/CreateCashier.cs`](https://github.com/yourusername/billing/blob/main/src/Billing/Cashiers/Commands/CreateCashier.cs)

This operation:

1. Validates the cashier data
2. Generates a unique `CashierId` (ULID)
3. Persists to the database
4. Publishes `CashierCreated` event

### Get Cashier by ID

**Endpoint**: `GET /api/cashiers/{cashierId}`

Retrieves a specific cashier's details.

```bash
curl http://localhost:8101/api/cashiers/csh_123456
```

**Query**: [`src/Billing/Cashiers/Queries/GetCashier.cs`](https://github.com/yourusername/billing/blob/main/src/Billing/Cashiers/Queries/GetCashier.cs)

### List All Cashiers

**Endpoint**: `GET /api/cashiers`

Returns a paginated list of all cashiers.

```bash
curl http://localhost:8101/api/cashiers?page=1&pageSize=20
```

**Query**: [`src/Billing/Cashiers/Queries/GetCashiers.cs`](https://github.com/yourusername/billing/blob/main/src/Billing/Cashiers/Queries/GetCashiers.cs)

### Update Cashier

**Endpoint**: `PUT /api/cashiers/{cashierId}`

Updates cashier information.

```bash
curl -X PUT http://localhost:8101/api/cashiers/csh_123456 \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Jane Smith",
    "email": "jane.smith@billing.com"
  }'
```

**Command**: [`src/Billing/Cashiers/Commands/UpdateCashier.cs`](https://github.com/yourusername/billing/blob/main/src/Billing/Cashiers/Commands/UpdateCashier.cs)

### Delete Cashier

**Endpoint**: `DELETE /api/cashiers/{cashierId}`

Removes a cashier from the system.

```bash
curl -X DELETE http://localhost:8101/api/cashiers/csh_123456
```

**Command**: [`src/Billing/Cashiers/Commands/DeleteCashier.cs`](https://github.com/yourusername/billing/blob/main/src/Billing/Cashiers/Commands/DeleteCashier.cs)

## Event Workflows

The Cashiers domain publishes integration events for key operations:

### CashierCreated Event

Published when a new cashier is added to the system.

<<< @/../src/Billing/Cashiers/Contracts/IntegrationEvents/CashierCreated.cs

### CashierUpdated Event

Published when cashier information is modified.

<<< @/../src/Billing/Cashiers/Contracts/IntegrationEvents/CashierUpdated.cs

### CashierDeleted Event

Published when a cashier is removed from the system.

<<< @/../src/Billing/Cashiers/Contracts/IntegrationEvents/CashierDeleted.cs

## Payment Processing

Cashiers track payment history through the `CashierPayment` relationship:

<<< @/../src/Billing/Cashiers/Contracts/Models/CashierPayment.cs

When an invoice is paid:

1. Payment is recorded against the invoice
2. CashierPayment entry is created
3. Payment totals are updated for reporting

## Database Schema

The cashiers data is stored in PostgreSQL:

**Table**: [`billing.cashiers`](https://github.com/yourusername/billing/blob/main/infra/Billing.Database/Liquibase/billing/tables/cashiers.sql)

**Stored Procedures**:

-   `billing.cashiers_create` - Create new cashier
-   `billing.cashiers_update` - Update cashier details
-   `billing.cashiers_delete` - Remove cashier
-   `billing.cashiers_get` - Retrieve single cashier
-   `billing.cashiers_list` - List all cashiers

## Testing Patterns

The Cashiers domain includes comprehensive test coverage:

### Unit Tests

Location: [`tests/Billing.Tests/Unit/Cashiers/`](https://github.com/yourusername/billing/tree/main/tests/Billing.Tests/Unit/Cashiers)

Example test structure:
<<< @/../tests/Billing.Tests/Unit/Cashier/CreateCashierCommandHandlerTests.cs

### Integration Tests

Integration tests verify the full stack including database operations and event publishing.

## Common Scenarios

### Onboarding a New Cashier

1. Create cashier via API
2. System generates unique ID
3. `CashierCreated` event published
4. Other services notified of new cashier
5. Cashier ready to process payments

### Cashier Deactivation

1. Call delete endpoint
2. Validate no pending payments
3. Mark as deleted in database
4. Publish `CashierDeleted` event
5. Historical data retained for audit

## Best Practices

1. **Validation**: Always validate email format and name requirements
2. **Event Handling**: Subscribe to cashier events for downstream processing
3. **Error Handling**: Use the `Result<T>` pattern for operation outcomes
4. **Testing**: Include both positive and negative test cases

## Troubleshooting

### Common Issues

**Duplicate Email**: The system prevents duplicate email addresses

-   Solution: Check for existing cashier before creation

**Referenced Cashier Deletion**: Cannot delete cashier with payment history

-   Solution: Implement soft delete or archive functionality

**Event Publishing Failures**: Integration events may fail to publish

-   Solution: Implement retry logic and dead letter queues

## Next Steps

-   Learn about [Invoice Processing](/guide/invoices/)
-   Understand [Event-Driven Architecture](/arch/events)
-   Explore [Testing Strategies](/arch/testing)
