---
title: PartitionKeyAttribute for message partitioning
description: Learn how to use the PartitionKeyAttribute to mark properties that should be used as partition keys in distributed messaging systems.
---

# PartitionKeyAttribute for message partitioning

The `PartitionKeyAttribute` is a marker attribute used to designate properties that should serve as partition keys in distributed messaging systems. This attribute helps ensure message ordering and efficient distribution across message broker partitions.

## Understanding PartitionKeyAttribute

When applied to a property, the `PartitionKeyAttribute` indicates that the property's value should be used as the partition key when publishing messages. This is crucial for:

- **Message ordering**: Messages with the same partition key are guaranteed to be processed in order
- **Load distribution**: Messages are distributed across partitions based on the key's hash
- **Consumer affinity**: Consumers can be assigned to specific partitions for consistent processing

## Basic usage

Apply the attribute to properties that represent logical grouping keys:

```csharp
using Operations.Extensions.Abstractions.Messaging;

[EventTopic("invoice-created", domain: "billing")]
public record InvoiceCreatedEvent(
    Guid InvoiceId,
    [PartitionKey] Guid TenantId,  // Messages grouped by tenant
    decimal Amount,
    DateTime CreatedAt);

[EventTopic("user-action", domain: "analytics")]
public record UserActionEvent(
    Guid ActionId,
    [PartitionKey] Guid UserId,    // Messages grouped by user
    string ActionType,
    DateTime Timestamp);
```

## Partitioning strategies

### Tenant-based partitioning
Group messages by tenant for multi-tenant applications:

```csharp
[EventTopic("ledger-entry-created", domain: "accounting")]
public record LedgerEntryCreatedEvent(
    Guid EntryId,
    [PartitionKey] Guid TenantId,  // All tenant events in same partition
    decimal Amount,
    string AccountCode);
```

### Entity-based partitioning  
Group messages by specific entities for ordered processing:

```csharp
[EventTopic("order-updated", domain: "sales")]
public record OrderUpdatedEvent(
    Guid EventId,
    [PartitionKey] Guid OrderId,   // All order events in same partition
    string Status,
    DateTime UpdatedAt);
```

### Geographic partitioning
Group messages by location or region:

```csharp
[EventTopic("sensor-reading", domain: "iot")]
public record SensorReadingEvent(
    Guid ReadingId,
    [PartitionKey] string RegionCode,  // Group by geographic region
    string SensorId,
    double Value,
    DateTime Timestamp);
```

## Integration with messaging frameworks

Messaging frameworks can use this attribute metadata to automatically extract partition keys:

```csharp
// Framework pseudocode for publishing
public async Task PublishEventAsync<T>(T eventData) where T : class
{
    var partitionKey = ExtractPartitionKey(eventData);
    var topic = GetTopicName<T>();
    
    await messageProducer.SendAsync(topic, eventData, partitionKey);
}

private string? ExtractPartitionKey<T>(T eventData)
{
    var partitionKeyProperty = typeof(T)
        .GetProperties()
        .FirstOrDefault(p => p.HasAttribute<PartitionKeyAttribute>());
        
    return partitionKeyProperty?.GetValue(eventData)?.ToString();
}
```

## Best practices

### Choose stable keys
Use properties that don't change frequently:

```csharp
// Good: Stable tenant identifier
[EventTopic("customer-updated")]
public record CustomerUpdatedEvent(
    Guid CustomerId,
    [PartitionKey] Guid TenantId,  // Stable - won't change
    string NewEmail);

// Avoid: Frequently changing properties
public record CustomerUpdatedEvent(
    Guid CustomerId,
    [PartitionKey] string Email,   // Changes when customer updates email
    string NewAddress);
```

### Consider cardinality
Balance between too few and too many unique partition keys:

```csharp
// Good: High cardinality for even distribution
[EventTopic("user-login")]
public record UserLoginEvent(
    [PartitionKey] Guid UserId,    // Many unique users
    DateTime LoginTime,
    string IpAddress);

// Consider impact: Low cardinality might create hot partitions
public record RegionalSalesEvent(
    Guid SaleId,
    [PartitionKey] string Region,  // Only few regions
    decimal Amount);
```

### Document partitioning strategy
Be explicit about why you chose a particular partition key:

```csharp
/// <summary>
/// Partitioned by TenantId to ensure all events for a tenant
/// are processed in order and by the same consumer instance.
/// </summary>
[EventTopic("billing-cycle-completed")]
public record BillingCycleCompletedEvent(
    Guid CycleId,
    [PartitionKey] Guid TenantId,  // Ensures tenant-level ordering
    DateTime CycleEndDate,
    decimal TotalAmount);
```

## Multiple properties consideration

While the attribute is designed for single properties, you might need composite keys:

```csharp
// If you need composite partitioning, create a computed property
[EventTopic("multi-tenant-user-action")]
public record MultiTenantUserActionEvent(
    Guid ActionId,
    Guid TenantId,
    Guid UserId,
    string ActionType)
{
    // Computed property for composite partitioning
    [PartitionKey]
    public string CompositeKey => $"{TenantId}:{UserId}";
}
```

## Integration example with Kafka

Here's how this might integrate with Kafka topic naming conventions:

```csharp
using Operations.Extensions.Abstractions.Messaging;

[EventTopic("cashier-activated", domain: "billing")]
public record CashierActivatedEvent(
    Guid CashierId,
    [PartitionKey] Guid TenantId,  // Used for Kafka partition key
    string CashierName,
    DateTime ActivatedAt);

// Framework usage:
// Topic: "billing.cashier-activated.v1"  
// Partition key: TenantId value
// Result: All events for same tenant go to same partition
```

## See also

- [EventTopicAttribute](event-topic-attribute.md)
- [DefaultDomainAttribute](default-domain-attribute.md)
- [Kafka Topic Naming Convention](kafka/topic-naming-convention.md)
- [Messaging Overview](overview.md)