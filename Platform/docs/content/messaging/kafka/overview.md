---
title: Kafka Messaging
description: Learn how to integrate Kafka messaging into your services using Platform Service Defaults.
---

# Kafka Messaging

The Platform provides seamless integration with Kafka, allowing your services to publish and consume messages efficiently. This section covers the basics of setting up Kafka messaging and utilizing its features within your application.

## Quick start

To enable Kafka messaging, you typically configure it as part of your Service Defaults setup:

```csharp
using Microsoft.Extensions.Hosting;
using Operations.ServiceDefaults;
using Operations.ServiceDefaults.Messaging.Kafka;

public class KafkaSetup
{
    public static void ConfigureKafka()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.AddServiceDefaults();

        // Add Kafka specific configuration
        builder.Services.AddKafka();

        var app = builder.Build();
        app.Run();
    }
}
```

## Key features

### Topic naming conventions

The Platform supports defining topic naming conventions to ensure consistency across your Kafka topics. This helps in organizing messages and simplifies consumer configurations.

```csharp
using Operations.ServiceDefaults.Messaging.Kafka;

// Example of a custom topic naming convention
public class MyCustomTopicNamingConvention : IKafkaTopicNamingConvention
{
    public string GetTopicName<T>()
    {
        return $"my-app.{typeof(T).Name.ToLowerInvariant()}";
    }
}

// Registering the custom convention
// builder.Services.AddSingleton<IKafkaTopicNamingConvention, MyCustomTopicNamingConvention>();
```

### Producing messages

Publish messages to Kafka topics using the `IMessageBus` interface, which is integrated with Kafka producers.

```csharp
using Wolverine;
using Operations.Extensions.Abstractions.Messaging;

public record UserCreated(Guid UserId, string UserName);

public class UserProducer
{
    private readonly IMessageBus _messageBus;

    public UserProducer(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    public async Task PublishUserCreated(Guid userId, string userName)
    {
        var userCreatedEvent = new UserCreated(userId, userName);
        await _messageBus.PublishAsync(userCreatedEvent);
        Console.WriteLine($"Published UserCreated event for user: {userName}");
    }
}
```

### Consuming messages

Define message handlers to consume messages from Kafka topics. Wolverine automatically discovers and registers these handlers.

```csharp
using Wolverine.Attributes;

public class UserCreatedHandler
{
    [Topic("my-app.usercreated")] // Specify the topic to consume from
    public void Handle(UserCreated message)
    {
        Console.WriteLine($"Received UserCreated event for user: {message.UserName} (ID: {message.UserId})");
        // Process the message
    }
}
```

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

*   [Wolverine Messaging](../wolverine-setup.md)
*   [CloudEvents](../cloudevents/overview.md)
*   [Telemetry](../telemetry/overview.md)
