---
title: Kafka Topic Naming Convention
description: Understand the standardized naming convention for Kafka topics used across services in the Operations platform.
---

# Kafka Topic Naming Convention

The `KafkaTopicNamingConvention` class provides a standardized and consistent way to name Kafka topics within the Operations platform. This convention ensures clarity, avoids conflicts, and simplifies the management of topics across different services and aggregates.

## Structure of Topic Names

Topic names follow a clear, hierarchical structure:

`{service}.{aggregate}.{version}`

-   **`service`**: Represents the logical service or bounded context (e.g., `accounting`, `billing`).
-   **`aggregate`**: Refers to the specific domain aggregate or entity within that service (e.g., `ledger`, `cashier`).
-   **`version`**: Indicates the version of the topic schema, allowing for schema evolution (currently `v1`).

## Defined Topics

The `KafkaTopicNamingConvention` class pre-defines topic names for various services and their aggregates:

### Accounting Service

-   **Ledger Topic**: `accounting.ledger.v1`
-   **Operation Topic**: `accounting.operation.v1`

### Billing Service

-   **Cashier Topic**: `billing.cashier.v1`
-   **Invoice Topic**: `billing.invoice.v1`

## Partition Keys

The `GetPartitionKey` method provides a standardized way to derive a Kafka partition key, typically from a `tenantId`. This ensures that messages for a specific tenant are routed to the same partition, maintaining order and simplifying consumption.

## Usage example

You can use these static properties directly in your Kafka producers and consumers to ensure you are interacting with the correct topics.

```csharp
using Operations.ServiceDefaults.Messaging.Kafka;

public class KafkaProducer
{
    public void SendAccountingLedgerMessage(string messageContent, string tenantId)
    {
        var topicName = KafkaTopicNamingConvention.Accounting.Ledger.Topic;
        var partitionKey = KafkaTopicNamingConvention.GetPartitionKey(tenantId);

        Console.WriteLine($"Sending message to topic: {topicName} with partition key: {partitionKey}");
        // Kafka producer logic here
    }

    public void SendBillingCashierMessage(string messageContent, string tenantId)
    {
        var topicName = KafkaTopicNamingConvention.Billing.Cashier.Topic;
        var partitionKey = KafkaTopicNamingConvention.GetPartitionKey(tenantId);

        Console.WriteLine($"Sending message to topic: {topicName} with partition key: {partitionKey}");
        // Kafka producer logic here
    }
}
```

## See also

- [Wolverine Setup Extensions](../wolverine-setup.md)
