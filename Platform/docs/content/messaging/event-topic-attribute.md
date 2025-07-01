---
title: EventTopicAttribute for message topic configuration
description: Learn how to use the EventTopicAttribute to configure topic names and domains for event messages in distributed systems.
---

# EventTopicAttribute for message topic configuration

The `EventTopicAttribute` provides a declarative way to configure topic names and domains for event messages in distributed messaging systems. This attribute enables automatic topic naming and routing based on your event definitions.

## Understanding EventTopicAttribute

The `EventTopicAttribute` can be applied to event classes to specify:
- **Topic name**: The specific topic where the event should be published
- **Domain**: The domain context for the event (optional, uses assembly default if not specified)

There are two variants:
- `EventTopicAttribute(string topic, string? domain = null)`: Explicit topic name
- `EventTopicAttribute<TEntity>(string? domain = null)`: Topic name derived from entity type

## Basic usage with explicit topic name

Apply the attribute to your event classes with an explicit topic name:

```csharp
using Operations.Extensions.Abstractions.Messaging;

[EventTopic("invoice-created", domain: "billing")]
public record InvoiceCreatedEvent(Guid InvoiceId, decimal Amount, DateTime CreatedAt);

[EventTopic("payment-processed", domain: "billing")]  
public record PaymentProcessedEvent(Guid PaymentId, Guid InvoiceId, decimal Amount);

[EventTopic("user-registered", domain: "identity")]
public record UserRegisteredEvent(Guid UserId, string Email, DateTime RegisteredAt);
```

## Generic usage with entity-based topic naming

Use the generic variant to automatically derive topic names from entity types:

```csharp
using Operations.Extensions.Abstractions.Messaging;

// Topic name will be "invoice" (kebab-case of "Invoice")
[EventTopic<Invoice>(domain: "billing")]
public record InvoiceUpdatedEvent(Guid InvoiceId, decimal NewAmount);

// Topic name will be "cashier" (kebab-case of "Cashier")  
[EventTopic<Cashier>(domain: "billing")]
public record CashierActivatedEvent(Guid CashierId, string Name);

// Topic name will be "ledger-entry" (kebab-case of "LedgerEntry")
[EventTopic<LedgerEntry>(domain: "accounting")]
public record LedgerEntryCreatedEvent(Guid EntryId, decimal Amount);

// Entity classes referenced
public class Invoice { }
public class Cashier { }  
public class LedgerEntry { }
```

## Domain resolution

The attribute supports flexible domain resolution:

### Using explicit domain
```csharp
// Always uses "billing" domain regardless of assembly default
[EventTopic("invoice-cancelled", domain: "billing")]
public record InvoiceCancelledEvent(Guid InvoiceId, string Reason);
```

### Using assembly default domain
```csharp
// Assumes assembly has [assembly: DefaultDomain(Domain = "billing")]

// Uses default domain from assembly
[EventTopic("cashier-suspended")]
public record CashierSuspendedEvent(Guid CashierId, string Reason);

// Also uses default domain from assembly  
[EventTopic<Invoice>()]
public record InvoiceDeletedEvent(Guid InvoiceId);
```

## Kebab-case conversion

The generic `EventTopicAttribute<TEntity>` automatically converts entity type names to kebab-case:

```csharp
// These conversions happen automatically:
// "Invoice" → "invoice"
// "LedgerEntry" → "ledger-entry"  
// "UserAccount" → "user-account"
// "PaymentMethod" → "payment-method"

[EventTopic<PaymentMethod>(domain: "billing")]
public record PaymentMethodAddedEvent(Guid UserId, string MethodType);
```

## Integration with messaging frameworks

The attribute metadata can be consumed by messaging frameworks for automatic topic routing:

```csharp
// Framework can read attribute metadata to determine:
// - Topic: "invoice-created"
// - Domain: "billing"  
// - Final topic name: "billing.invoice-created.v1" (depending on naming convention)

[EventTopic("invoice-created", domain: "billing")]
public record InvoiceCreatedEvent(Guid InvoiceId, decimal Amount);
```

## Best practices

- **Use descriptive topic names** that clearly indicate the event purpose
- **Follow naming conventions** like kebab-case for consistency
- **Group related events** by domain for better organization
- **Version your events** to handle schema evolution
- **Document event contracts** to ensure consumer understanding
- **Use the generic variant** when the topic name should match the entity name

## Complex example

Here's a comprehensive example showing different usage patterns:

```csharp
using Operations.Extensions.Abstractions.Messaging;

// Assembly-level default domain
[assembly: DefaultDomain(Domain = "billing")]

namespace MyBilling.Events
{
    // Explicit topic with explicit domain
    [EventTopic("subscription-upgraded", domain: "subscriptions")]
    public record SubscriptionUpgradedEvent(Guid SubscriptionId, string NewPlan);

    // Explicit topic with default domain (billing)
    [EventTopic("invoice-payment-failed")]
    public record InvoicePaymentFailedEvent(Guid InvoiceId, string FailureReason);

    // Entity-based topic with explicit domain
    [EventTopic<Customer>(domain: "crm")]
    public record CustomerUpdatedEvent(Guid CustomerId, string NewEmail);

    // Entity-based topic with default domain (billing)
    [EventTopic<Invoice>()]
    public record InvoiceGeneratedEvent(Guid InvoiceId, Guid CustomerId, decimal Amount);
}

// Referenced entity types
public class Customer { }
public class Invoice { }
```

## See also

- [DefaultDomainAttribute](default-domain-attribute.md)
- [PartitionKeyAttribute](partition-key-attribute.md)
- [Kafka Topic Naming Convention](kafka/topic-naming-convention.md)
- [String Extensions](../extensions/string-extensions.md) (for kebab-case conversion)