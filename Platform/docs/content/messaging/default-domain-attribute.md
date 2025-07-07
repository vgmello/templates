---
title: DefaultDomainAttribute for messaging domain configuration
description: Learn how to use the DefaultDomainAttribute to set a default domain for messaging components at the assembly level.
---

# DefaultDomainAttribute for messaging domain configuration

The `DefaultDomainAttribute` provides a way to configure a default domain name for messaging components at the assembly level. This attribute is particularly useful in distributed systems where you need to organize message topics and event handling by domain boundaries.

## Understanding DefaultDomainAttribute

The `DefaultDomainAttribute` is applied at the assembly level and allows you to specify a default domain name that can be used by messaging frameworks for topic naming conventions, event routing, and other domain-specific configurations.

Key characteristics:
- **Assembly-level attribute**: Applied using `[assembly: DefaultDomain(...)]`
- **Single domain per assembly**: Each assembly can have one default domain
- **Framework integration**: Used by messaging frameworks to determine topic names and routing

## Usage example

Apply the attribute at the assembly level, typically in your `AssemblyInfo.cs` file or at the top of your `Program.cs`:

```csharp
using Operations.Extensions.Abstractions.Messaging;

// Set the default domain for this assembly
[assembly: DefaultDomain(Domain = "billing")]

namespace MyBilling.Api
{
    // Your application code here
    public class Program
    {
        public static void Main(string[] args)
        {
            // Application startup
        }
    }
}
```

## Integration with EventTopicAttribute

When used in conjunction with `EventTopicAttribute`, the default domain provides a fallback when no explicit domain is specified:

```csharp
// This assembly has [assembly: DefaultDomain(Domain = "billing")]

// Uses the default domain "billing" for topic naming
[EventTopic("invoice-created")]
public record InvoiceCreatedEvent(Guid InvoiceId, decimal Amount);

// Explicitly overrides the default domain
[EventTopic("payment-processed", domain: "payments")]
public record PaymentProcessedEvent(Guid PaymentId, Guid InvoiceId);
```

In this example:
- `InvoiceCreatedEvent` will use the default domain "billing"
- `PaymentProcessedEvent` explicitly specifies the "payments" domain

## Domain-based topic organization

The default domain helps organize your messaging topology by bounded context:

```csharp
// Billing assembly
[assembly: DefaultDomain(Domain = "billing")]

// All events in this assembly use "billing" domain by default
[EventTopic("cashier-created")]
public record CashierCreatedEvent(Guid CashierId, string Name);

[EventTopic("invoice-generated")]  
public record InvoiceGeneratedEvent(Guid InvoiceId, Guid CashierId);
```

```csharp
// Accounting assembly  
[assembly: DefaultDomain(Domain = "accounting")]

// All events in this assembly use "accounting" domain by default
[EventTopic("ledger-entry-created")]
public record LedgerEntryCreatedEvent(Guid EntryId, decimal Amount);
```

This creates a clear separation of concerns and helps with message routing in distributed systems.

## Best practices

- **Use meaningful domain names** that reflect your bounded contexts
- **Keep domains consistent** with your domain-driven design boundaries  
- **Avoid generic names** like "app" or "system" - be specific
- **Document domain boundaries** to ensure team understanding
- **Consider message versioning** when evolving domain structures

## See also

- [EventTopicAttribute](event-topic-attribute.md)
- [PartitionKeyAttribute](partition-key-attribute.md)
- [Kafka Topic Naming Convention](kafka/topic-naming-convention.md)