---
title: Kafka Messaging
description: Learn how to integrate Kafka messaging into your services using Platform Service Defaults.
---

# Kafka Messaging

The Platform provides seamless integration with Kafka, allowing your services to publish and consume messages efficiently. This section covers the basics of setting up Kafka messaging and utilizing its features within your application.

## Quick start

To enable Kafka messaging, you typically configure it as part of your Service Defaults setup:

[!code-csharp[](~/docs/samples/messaging/kafka/KafkaSetup.cs)]

## Key features

### Topic naming conventions

The Platform supports defining topic naming conventions to ensure consistency across your Kafka topics. This helps in organizing messages and simplifies consumer configurations.

[!code-csharp[](~/docs/samples/messaging/kafka/MyCustomTopicNamingConvention.cs)]

### Producing messages

Publish messages to Kafka topics using the `IMessageBus` interface, which is integrated with Kafka producers.

[!code-csharp[](~/docs/samples/messaging/kafka/UserProducer.cs)]

### Consuming messages

Define message handlers to consume messages from Kafka topics. Wolverine automatically discovers and registers these handlers.

[!code-csharp[](~/docs/samples/messaging/kafka/UserCreatedHandler.cs)]

## Configuration

You can further configure Kafka settings, such as broker addresses, consumer groups, and security settings, through your application's configuration files.

```json
{
  "Kafka": {
    "Brokers": "localhost:9092",
    "ConsumerGroup": "my-app-group",
    "SecurityProtocol": "Plaintext"
  }
}
```

## See also

*   [Wolverine Messaging](wolverine-integration.md)
*   [CloudEvents](cloudevents.md)
*   [Telemetry](telemetry.md)
