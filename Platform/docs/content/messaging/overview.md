---
title: Messaging
description: Event-driven architecture with Wolverine messaging, CQRS patterns, CloudEvents, and reliable message processing.
ms.date: 01/27/2025
---

# Messaging

The Platform provides a comprehensive messaging foundation built on Wolverine, enabling event-driven architecture with CQRS patterns, reliable message delivery, and seamless integration with PostgreSQL and Kafka. You get production-ready messaging infrastructure with minimal configuration.

## Quick start

Enable messaging in your application with built-in reliability and observability:

[!code-csharp[](~/samples/service-defaults/ServiceDefaultsSetup.cs#ServiceDefaultsConfiguration)]

This setup provides:
- **CQRS command and query handling** with automatic discovery
- **PostgreSQL persistence** for reliable message delivery
- **Kafka integration** for external messaging
- **CloudEvents** for standardized event formats
- **Built-in middleware** for validation, telemetry, and error handling
- **Transactional outbox pattern** for consistency

## Core concepts

### Command and Query Separation (CQRS)
Separate commands (write operations) from queries (read operations):

[!code-csharp[](~/samples/messaging/CqrsPattern.cs#CommandDefinition)]
[!code-csharp[](~/samples/messaging/CqrsPattern.cs#QueryDefinition)]

CQRS benefits:
- **Clear separation** of concerns
- **Independent scaling** of read and write operations
- **Optimized data models** for specific use cases
- **Event sourcing** capabilities

### Message handlers
Implement handlers for commands, queries, and events:

[!code-csharp[](~/samples/messaging/CqrsPattern.cs#CommandHandler)]
[!code-csharp[](~/samples/messaging/CqrsPattern.cs#QueryHandler)]

Handler features:
- **Automatic discovery** via assembly scanning
- **Dependency injection** support
- **Return value handling** for queries
- **Side effect management** for commands

### Event-driven communication
Publish events for loosely coupled communication:

[!code-csharp[](~/samples/messaging/CqrsPattern.cs#EventDefinition)]

Event benefits:
- **Loose coupling** between services
- **Scalable architecture** with async processing
- **Audit trail** of system changes
- **Integration** with external systems

## PostgreSQL persistence

### Reliable messaging
Messages are persisted to PostgreSQL for guaranteed delivery:

[!code-csharp[](~/samples/messaging/PostgresqlPersistence.cs)]

Reliability features:
- **Transactional outbox** pattern
- **Automatic retries** with exponential backoff
- **Dead letter queues** for failed messages
- **Message deduplication** to prevent duplicate processing

### Queue configuration
Configure message queues for optimal performance:

[!code-csharp[](~/samples/messaging/QueueConfiguration.cs)]

Queue features:
- **Inbound/outbound** queue separation
- **Configurable concurrency** limits
- **Message prioritization** support
- **Automatic provisioning** of database objects

### Schema management
Wolverine automatically manages database schema:

[!code-csharp[](~/samples/messaging/SchemaManagement.cs)]

Schema features:
- **Per-service schemas** for isolation
- **Automatic provisioning** of tables and functions
- **Migration support** for schema changes
- **Multi-tenant** schema patterns

## Kafka integration

### External messaging
Connect to Kafka for inter-service communication:

[!code-csharp[](~/samples/messaging/KafkaIntegration.cs)]

Kafka features:
- **High-throughput** message processing
- **Topic-based** message routing
- **Consumer group** management
- **Health checks** for monitoring

### Topic naming conventions
Consistent topic naming across services:

[!code-csharp[](~/samples/messaging/TopicNaming.cs)]

Naming benefits:
- **Consistent** topic structure
- **Service-specific** namespace isolation
- **Event type** identification
- **Version management** support

## CloudEvents support

### Standardized event format
Use CloudEvents specification for interoperability:

[!code-csharp[](~/samples/messaging/CloudEventsFormat.cs)]

CloudEvents provides:
- **Standard metadata** for all events
- **Interoperability** with other systems
- **Tracing correlation** across services
- **Content negotiation** support

### Event metadata
Rich metadata for event processing:

[!code-csharp[](~/samples/messaging/EventMetadata.cs)]

Metadata includes:
- **Event source** identification
- **Event type** classification
- **Trace correlation** IDs
- **Timestamp** information

## Middleware pipeline

### Built-in middleware
The Platform includes essential middleware components:

[!code-csharp[](~/samples/messaging/BuiltInMiddleware.cs)]

Middleware provides:
- **Exception handling** with retry policies
- **FluentValidation** for input validation
- **Performance monitoring** with metrics
- **OpenTelemetry** integration for tracing
- **CloudEvents** transformation

### Custom middleware
Add application-specific middleware:

[!code-csharp[](~/samples/messaging/CustomMiddleware.cs)]

Custom middleware enables:
- **Authorization** checks
- **Custom logging** and auditing
- **Rate limiting** policies
- **Circuit breaker** patterns

### Middleware ordering
Control middleware execution order:

[!code-csharp[](~/samples/messaging/MiddlewareOrdering.cs)]

Ordering considerations:
- **Authentication** before authorization
- **Validation** before business logic
- **Logging** spans entire pipeline
- **Error handling** wraps other middleware

## Error handling and resilience

### Exception policies
Configure how different exceptions are handled:

[!code-csharp[](~/samples/messaging/ExceptionPolicies.cs)]

Exception handling features:
- **Retry policies** with exponential backoff
- **Dead letter queues** for persistent failures
- **Custom error responses** for validation failures
- **Structured logging** of errors

### Circuit breaker patterns
Prevent cascade failures with circuit breakers:

[!code-csharp[](~/samples/messaging/CircuitBreaker.cs)]

Circuit breaker benefits:
- **Fail fast** during outages
- **Automatic recovery** detection
- **Cascade failure** prevention
- **Graceful degradation** support

### Saga patterns
Coordinate long-running processes:

[!code-csharp[](~/samples/messaging/SagaPatterns.cs)]

Saga features:
- **State persistence** across steps
- **Compensation** for rollback scenarios
- **Timeout handling** for stuck processes
- **Event-driven** state transitions

## Performance optimization

### Message batching
Process messages in batches for better throughput:

[!code-csharp[](~/samples/messaging/MessageBatching.cs)]

Batching benefits:
- **Higher throughput** for bulk operations
- **Reduced database** roundtrips
- **Better resource** utilization
- **Configurable batch** sizes

### Connection pooling
Efficient database connection management:

[!code-csharp[](~/samples/messaging/ConnectionPooling.cs)]

Connection pooling provides:
- **Resource efficiency** with shared connections
- **Automatic scaling** based on load
- **Connection health** monitoring
- **Timeout management** for idle connections

### Serialization optimization
Efficient message serialization:

[!code-csharp[](~/samples/messaging/SerializationOptimization.cs)]

Serialization features:
- **System.Text.Json** for performance
- **Custom converters** for complex types
- **Minimal allocations** during processing
- **Schema evolution** support

## Testing strategies

### Unit testing handlers
Test message handlers in isolation:

[!code-csharp[](~/samples/messaging/UnitTestingHandlers.cs)]

### Integration testing
Test complete message flows:

[!code-csharp[](~/samples/messaging/IntegrationTesting.cs)]

### Test doubles
Use test doubles for external dependencies:

[!code-csharp[](~/samples/messaging/TestDoubles.cs)]

## Configuration options

### Environment-specific settings
Configure messaging for different environments:

[!code-csharp[](~/samples/messaging/EnvironmentConfiguration.cs)]

### Service bus options
Comprehensive configuration options:

[!code-csharp[](~/samples/messaging/ServiceBusOptions.cs)]

Configuration includes:
- **Connection strings** for databases and Kafka
- **Queue settings** for throughput and concurrency
- **Retry policies** for error handling
- **Serialization options** for message formats

## Observability integration

### Metrics collection
Automatic metrics for message processing:

[!code-csharp[](~/samples/messaging/MetricsCollection.cs)]

Metrics include:
- **Message throughput** rates
- **Processing latency** histograms
- **Error rates** by message type
- **Queue depth** monitoring

### Distributed tracing
End-to-end tracing across services:

[!code-csharp[](~/samples/messaging/DistributedTracing.cs)]

Tracing provides:
- **Request correlation** across services
- **Performance bottleneck** identification
- **Error propagation** tracking
- **Service dependency** mapping

## Best practices

- **Use CQRS** to separate read and write concerns
- **Implement idempotent** message handlers
- **Design for eventual** consistency
- **Use CloudEvents** for interoperability
- **Configure appropriate** retry policies
- **Monitor message** processing metrics
- **Test message handlers** thoroughly
- **Version your** message contracts

## Common patterns

### Event sourcing
Store events as the source of truth:

[!code-csharp[](~/samples/messaging/EventSourcing.cs)]

### Outbox pattern
Ensure consistency between database updates and message publishing:

[!code-csharp[](~/samples/messaging/OutboxPattern.cs)]

### Request-response pattern
Implement synchronous request-response over messaging:

[!code-csharp[](~/samples/messaging/RequestResponse.cs)]

## Migration strategies

### From direct database calls
Gradually introduce messaging patterns:

[!code-csharp[](~/samples/messaging/MigrationFromDatabase.cs)]

### From other messaging systems
Migrate from existing messaging infrastructure:

[!code-csharp[](~/samples/messaging/MigrationFromOtherSystems.cs)]

## Next steps

- Learn about [Wolverine Integration](wolverine-integration.md) in detail
- Explore [CloudEvents](cloudevents.md) for standardized events
- Understand [Kafka](kafka.md) configuration and patterns
- Review [Middlewares](middlewares.md) for cross-cutting concerns
- Study [Telemetry](telemetry.md) for monitoring and observability

## Additional resources

- [Wolverine Documentation](https://wolverine.netlify.app/)
- [CloudEvents Specification](https://cloudevents.io/)
- [Apache Kafka Documentation](https://kafka.apache.org/documentation/)
- [CQRS Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs)