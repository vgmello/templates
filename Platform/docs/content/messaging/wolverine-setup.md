---
title: Wolverine Setup Extensions
description: Learn how to configure Wolverine for robust messaging, persistence, and policies using the provided setup extensions.
---

# Wolverine Setup Extensions

The `WolverineSetupExtensions` class provides a comprehensive set of extension methods to integrate and configure Wolverine, a powerful message bus and command processing library, into your ASP.NET Core applications. These extensions streamline the setup of messaging infrastructure, including persistence, policies, and integration with other services like Kafka.

## Key Features

### AddWolverine

This is the primary extension method to add Wolverine to your application. It sets up Wolverine with a default configuration, including PostgreSQL persistence (if a connection string is provided) and various messaging policies.

#### Usage example

Call `AddWolverine` on your `IHostApplicationBuilder`:

[!code-csharp[](~/samples/messaging/wolverine-setup/Program.cs?highlight=3)]

### AddWolverineWithDefaults

This method provides the detailed configuration for Wolverine, which is internally called by `AddWolverine`. It sets up:

-   **Service Bus Options**: Configures `ServiceBusOptions` for service naming and connection strings.
-   **Serialization**: Uses System.Text.Json for message serialization.
-   **PostgreSQL Persistence**: Configures durable messaging and transport using PostgreSQL if a connection string is available.
-   **Messaging Policies**: Adds various policies for exception handling, validation, request performance, OpenTelemetry instrumentation, and CloudEvents.
-   **Kafka Integration**: Sets up Kafka transport and a Kafka health check if a Kafka connection string is provided.
-   **Application Handlers**: Discovers message handlers from the entry assembly and any domain-marked assemblies.

#### Usage example

You typically won't call this directly, as `AddWolverine` handles it. However, if you need to customize the Wolverine options, you can pass a configuration action:

```csharp
builder.AddWolverine(opts =>
{
    // Add custom Wolverine options here
    opts.Policies.Add<MyCustomPolicy>();
});
```

### ConfigureAppHandlers

This method helps Wolverine discover message handlers within your application. It scans the entry assembly and any assemblies marked with `DomainAssemblyAttribute` to include them in Wolverine's message discovery process.

#### Usage example

This is called internally by `AddWolverineWithDefaults`.

### ConfigurePostgresql

This extension configures Wolverine to use PostgreSQL for message persistence and transport. It sets up schema names and enables auto-provisioning of necessary database objects.

#### Usage example

This is called internally by `AddWolverineWithDefaults` if a PostgreSQL connection string is present.

### ConfigureReliableMessaging

This method applies policies to ensure reliable messaging, including automatic transaction application, durable local queues, and durable outbox for all sending endpoints.

#### Usage example

This is called internally by `AddWolverineWithDefaults` if a PostgreSQL connection string is present.

## Configuration

Wolverine's behavior is heavily influenced by connection strings in your `appsettings.json`:

-   **ServiceBus**: Used for PostgreSQL persistence and transport.
-   **Messaging**: Used for Kafka integration.

### Example appsettings.json for Wolverine

```json
{
  "ConnectionStrings": {
    "ServiceBus": "Host=localhost;Port=5432;Database=wolverine_db;Username=postgres;Password=password",
    "Messaging": "localhost:9092" // For Kafka
  }
}
```

## See also

- [Service Bus Options](./service-bus-options.md)
- [CloudEvents Middleware](./cloudevents/overview.md)
- [Exception Handling Frame](./middlewares/overview.md#exception-handling-frame)
- [Fluent Validation Executor](./middlewares/overview.md#fluent-validation-executor)
- [Request Performance Middleware](./middlewares/overview.md#request-performance-middleware)
- [OpenTelemetry Instrumentation Middleware](./middlewares/overview.md#opentelemetry-instrumentation-middleware)
