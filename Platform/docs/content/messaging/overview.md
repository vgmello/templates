# Messaging Overview

The Platform provides a comprehensive messaging infrastructure built on Wolverine with PostgreSQL persistence, Kafka transport, and advanced middleware patterns for reliable, observable, and high-performance message processing.

## Key Benefits

### 🚀 **Enterprise Messaging Patterns**
- **Transactional outbox** - Guaranteed message delivery with database consistency
- **Automatic retries** - Configurable retry policies with exponential backoff
- **Dead letter queues** - Automatic handling of failed messages
- **Message routing** - Intelligent message distribution across services

### 🎯 **CQRS/Event Sourcing Ready**
- **Command/Query separation** - Clear distinction between read and write operations
- **Event-driven architecture** - Publish/subscribe patterns for loose coupling
- **Saga orchestration** - Long-running business processes
- **Domain events** - Rich business event modeling

### 🔧 **Developer Experience**
- **Type-safe messaging** - Strongly typed commands and queries
- **Auto-discovery** - Automatic handler registration from domain assemblies
- **Middleware pipeline** - Composable cross-cutting concerns
- **Testing support** - Built-in test harnesses and mocking capabilities

## Core Architecture

### Message Types

The Platform distinguishes between different message patterns:

```csharp
// Commands - Write operations that change state
public class CreateCashierCommand : ICommand<CashierDto>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Currencies { get; set; } = new();
}

// Queries - Read operations that return data
public class GetCashierQuery : IQuery<CashierDto>
{
    public Guid CashierId { get; set; }
}

// Integration Events - Cross-service notifications
namespace Billing.Contracts.Cashier.IntegrationEvents
{
    public class CashierCreated
    {
        public Guid CashierId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Currencies { get; set; } = new();
        public DateTimeOffset CreatedAt { get; set; }
    }
}

// Domain Events - Internal service events
public class InvoiceGenerated
{
    public Guid InvoiceId { get; set; }
    public Guid CashierId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
}
```

### Handler Implementation

```csharp
// Command Handler
public class CreateCashierCommandHandler : ICommandHandler<CreateCashierCommand, CashierDto>
{
    private readonly IMessageBus _messageBus;
    private readonly DbDataSource _dataSource;

    public async Task<CashierDto> ExecuteAsync(
        CreateCashierCommand command, 
        CancellationToken cancellationToken)
    {
        // Transactional operation with automatic outbox
        using var transaction = await _dataSource.BeginTransactionAsync(cancellationToken);
        
        var cashier = await _dataSource.SpExecute(
            "cashier_create", 
            command.ToDbParams(), 
            cancellationToken);

        // Publish integration event (goes to outbox automatically)
        await _messageBus.PublishAsync(new CashierCreated
        {
            CashierId = cashier.Id,
            Name = cashier.Name,
            Email = cashier.Email,
            Currencies = cashier.Currencies,
            CreatedAt = cashier.CreatedAt
        }, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return cashier;
    }
}

// Query Handler
public class GetCashierQueryHandler : IQueryHandler<GetCashierQuery, CashierDto>
{
    private readonly DbDataSource _dataSource;

    public async Task<CashierDto> ExecuteAsync(
        GetCashierQuery query, 
        CancellationToken cancellationToken)
    {
        var result = await _dataSource.SpQuery<CashierDto>(
            "cashier_get", 
            query.ToDbParams(), 
            cancellationToken);
            
        return result.FirstOrDefault() ?? throw new CashierNotFoundException(query.CashierId);
    }
}

// Integration Event Handler
public class CashierCreatedHandler : IIntegrationEventHandler<CashierCreated>
{
    private readonly ILogger<CashierCreatedHandler> _logger;

    public async Task HandleAsync(CashierCreated @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cashier {CashierId} was created in billing service", @event.CashierId);
        
        // Update read models, send notifications, etc.
        await UpdateCashierReadModel(@event);
        await SendWelcomeEmail(@event);
    }
}
```

## Wolverine Configuration

### Basic Setup

```csharp
public static IServiceCollection AddWolverine(this IServiceCollection services, IConfiguration configuration)
{
    services.AddWolverine(opts =>
    {
        // PostgreSQL persistence
        opts.PersistMessagesWithPostgresql(connectionString: "ServiceBus");
        
        // Service discovery integration
        opts.Services.AddServiceDiscovery();
        opts.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddServiceDiscovery();
        });

        // Configure handlers from domain assemblies
        opts.ConfigureAppHandlers();
        
        // Add middleware pipeline
        opts.Policies.Add<ExceptionHandlingPolicy>();
        opts.Policies.Add<FluentValidationPolicy>();
        opts.Policies.Add(new RequestPerformancePolicy());
        opts.Policies.Add(new OpenTelemetryInstrumentationPolicy());
        
        // Kafka transport
        opts.UseKafka(configuration.GetConnectionString("Messaging"))
            .ConfigureTopics(topics =>
            {
                // Tenant-partitioned topics
                topics.Create("billing.cashier.v1", 3)
                      .WithPartitionKey<CashierCreated>(x => x.TenantId);
                      
                topics.Create("billing.invoice.v1", 5)
                      .WithPartitionKey<InvoicePaid>(x => x.TenantId);
            });
    });

    return services;
}
```

### Advanced Configuration

```csharp
public static WolverineOptions ConfigurePostgresql(this WolverineOptions opts, string connectionString)
{
    opts.PersistMessagesWithPostgresql(connectionString, schema: "wolverine")
        .ConfigureWith(settings =>
        {
            // Transactional inbox/outbox
            settings.CommandBusRequiredTransaction = true;
            settings.EventBusRequiredTransaction = true;
            
            // Performance tuning
            settings.ScheduledJobLockId = 1; // Unique per service
            settings.ScheduledJobPollingTime = TimeSpan.FromSeconds(5);
            settings.NodeReassignmentPollingTime = TimeSpan.FromMinutes(1);
            
            // Auto-provisioning
            settings.AutoProvision = true;
            settings.SchemaName = GetServiceSchemaName();
        });

    // Configure queue behaviors
    opts.LocalQueueFor<ProcessInvoiceCommand>()
        .MaximumParallelization(5)
        .Sequential() // Process invoices in order
        .UseDurableInbox(); // Guaranteed processing

    opts.LocalQueueFor<SendEmailCommand>()
        .MaximumParallelization(20)
        .UseDurableInbox()
        .CircuitBreaker(5, TimeSpan.FromMinutes(1)); // Protect external email service

    return opts;
}
```

## Type-Safe Message Bus

### Extension Methods

```csharp
public static class MessageBusExtensions
{
    public static async Task<TResult> InvokeCommandAsync<TResult>(
        this IMessageBus bus,
        ICommand<TResult> command,
        CancellationToken cancellationToken = default)
    {
        return await bus.InvokeAsync<TResult>(command, cancellationToken);
    }

    public static async Task<TResult> InvokeQueryAsync<TResult>(
        this IMessageBus bus,
        IQuery<TResult> query,
        CancellationToken cancellationToken = default)
    {
        return await bus.InvokeAsync<TResult>(query, cancellationToken);
    }
}
```

### Usage in Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class CashiersController : ControllerBase
{
    private readonly IMessageBus _messageBus;

    public CashiersController(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    [HttpGet]
    public async Task<ActionResult<GetCashiersResult>> GetCashiers(
        CancellationToken cancellationToken = default)
    {
        // Type-safe query execution
        var result = await _messageBus.InvokeQueryAsync(
            new GetCashiersQuery(), 
            cancellationToken);
            
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CashierDto>> CreateCashier(
        [FromBody] CreateCashierRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateCashierCommand
        {
            Name = request.Name,
            Email = request.Email,
            Currencies = request.Currencies
        };

        // Type-safe command execution
        var result = await _messageBus.InvokeCommandAsync(command, cancellationToken);
        
        return CreatedAtAction(nameof(GetCashier), new { id = result.Id }, result);
    }
}
```

## Middleware Pipeline

### Exception Handling

```csharp
public class ExceptionHandlingPolicy : IChainPolicy
{
    public void Apply(IReadOnlyList<IChain> chains, GenerationRules rules, IContainer container)
    {
        foreach (var chain in chains)
        {
            if (chain.IsForProjection()) continue;
            
            chain.Middleware.Add(new ExceptionHandlingFrame());
        }
    }
}

public class ExceptionHandlingFrame : AsyncFrame
{
    public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
    {
        writer.Write("try");
        writer.Write("{");
        
        Next?.GenerateCode(method, writer);
        
        writer.Write("}");
        writer.Write("catch (System.Exception ex)");
        writer.Write("{");
        writer.Write($"await {typeof(ExceptionHandlingService).FullName}.HandleExceptionAsync(ex, context);");
        writer.Write("throw;");
        writer.Write("}");
    }
}
```

### Validation

```csharp
public class FluentValidationPolicy : IChainPolicy
{
    public void Apply(IReadOnlyList<IChain> chains, GenerationRules rules, IContainer container)
    {
        foreach (var chain in chains.Where(x => x.InputType() != null))
        {
            var inputType = chain.InputType()!;
            var validatorTypes = container.Model.For<IValidator>()
                .Instances
                .Where(x => x.ImplementationType.IsGenericType &&
                           x.ImplementationType.GetGenericTypeDefinition() == typeof(IValidator<>) &&
                           x.ImplementationType.GetGenericArguments()[0] == inputType)
                .ToList();

            if (validatorTypes.Any())
            {
                chain.Middleware.Add(new FluentValidationResultFrame(inputType));
            }
        }
    }
}
```

### Performance Monitoring

```csharp
public class RequestPerformanceMiddleware
{
    public async Task BeforeAsync(IMessageContext context, CancellationToken cancellation)
    {
        var messageName = context.Envelope.GetMessageName();
        var logger = context.Services.GetRequiredService<ILogger<RequestPerformanceMiddleware>>();
        
        logger.LogInformation("Processing {MessageType} {MessageId}", 
            messageName, context.Envelope.Id);

        var meter = context.Services.GetRequiredService<MessagingMeterStore>()
            .GetMeter(messageName);
            
        meter.MessageCounter.Add(1, new KeyValuePair<string, object?>("message.type", messageName));
        
        context.Storage[StorageKeys.Stopwatch] = Stopwatch.StartNew();
        context.Storage[StorageKeys.MessageMeter] = meter;
    }

    public async Task FinallyAsync(IMessageContext context, CancellationToken cancellation)
    {
        if (context.Storage.TryGetValue(StorageKeys.Stopwatch, out var stopwatchObj) &&
            stopwatchObj is Stopwatch stopwatch)
        {
            var messageName = context.Envelope.GetMessageName();
            var duration = stopwatch.ElapsedMilliseconds;
            
            var logger = context.Services.GetRequiredService<ILogger<RequestPerformanceMiddleware>>();
            
            if (context.Storage.ContainsKey(StorageKeys.Exception))
            {
                logger.LogError("Message {MessageType} {MessageId} failed after {Duration}ms",
                    messageName, context.Envelope.Id, duration);
                    
                if (context.Storage.TryGetValue(StorageKeys.MessageMeter, out var meterObj) &&
                    meterObj is MessagingMetrics meter)
                {
                    meter.ExceptionCounter.Add(1, 
                        new KeyValuePair<string, object?>("message.type", messageName),
                        new KeyValuePair<string, object?>("exception.type", 
                            context.Storage[StorageKeys.Exception]?.GetType().Name));
                }
            }
            else
            {
                logger.LogInformation("Message {MessageType} {MessageId} completed in {Duration}ms",
                    messageName, context.Envelope.Id, duration);
            }

            if (context.Storage.TryGetValue(StorageKeys.MessageMeter, out var messageMeterObj) &&
                messageMeterObj is MessagingMetrics messageMeter)
            {
                messageMeter.DurationHistogram.Record(duration, 
                    new KeyValuePair<string, object?>("message.type", messageName));
            }
        }
    }
}
```

## Cloud Events Integration

### Automatic CloudEvents Wrapping

```csharp
public class CloudEventMiddleware
{
    public Task BeforeAsync(IMessageContext context, CancellationToken cancellation)
    {
        var messageType = context.Envelope.MessageType;
        
        // Wrap integration events in CloudEvents format
        if (IsIntegrationEvent(messageType))
        {
            var cloudEvent = new CloudEvent
            {
                Type = $"com.company.{GetServiceName()}.{messageType.Name.ToLowerInvariant()}",
                Source = new Uri($"urn:service:{GetServiceName()}"),
                Id = context.Envelope.Id.ToString(),
                Time = DateTimeOffset.UtcNow,
                DataContentType = "application/json",
                Data = context.Envelope.Message
            };

            context.Envelope.Message = cloudEvent;
            context.Envelope.Headers["ce-specversion"] = "1.0";
            context.Envelope.Headers["ce-type"] = cloudEvent.Type;
            context.Envelope.Headers["ce-source"] = cloudEvent.Source.ToString();
        }

        return Task.CompletedTask;
    }

    private static bool IsIntegrationEvent(Type messageType)
    {
        return messageType.Namespace?.EndsWith(".IntegrationEvents") == true;
    }
}
```

### Benefits
- **Standardized format** - CloudEvents v1.0 compliance
- **Interoperability** - Works with any CloudEvents-compatible system
- **Automatic metadata** - Source, type, and timing information
- **Event sourcing ready** - Complete event history with metadata

## Kafka Integration

### Topic Naming Convention

```csharp
public class KafkaTopicNamingConvention
{
    private static readonly Dictionary<string, List<string>> ServiceTopics = new()
    {
        ["accounting"] = new() { "ledger", "operation" },
        ["billing"] = new() { "cashier", "invoice" }
    };

    public static string GetTopicName(string serviceName, string aggregate, string version = "v1")
    {
        return $"{serviceName.ToLowerInvariant()}.{aggregate.ToLowerInvariant()}.{version}";
    }

    public static int GetPartitionKey(string tenantId)
    {
        // Consistent hashing for tenant isolation
        return Math.Abs(tenantId.GetHashCode()) % MaxPartitions;
    }
}
```

### Multi-Tenant Partitioning

```csharp
// Configure Kafka with tenant partitioning
opts.UseKafka(connectionString)
    .ConfigureTopics(topics =>
    {
        topics.Create("billing.cashier.v1", partitions: 5)
              .WithPartitionKey<CashierCreated>(evt => GetTenantPartition(evt.TenantId))
              .WithPartitionKey<CashierUpdated>(evt => GetTenantPartition(evt.TenantId));
              
        topics.Create("billing.invoice.v1", partitions: 10)
              .WithPartitionKey<InvoicePaid>(evt => GetTenantPartition(evt.TenantId))
              .WithPartitionKey<InvoiceCancelled>(evt => GetTenantPartition(evt.TenantId));
    });

private static string GetTenantPartition(string tenantId)
{
    // Ensures all events for a tenant go to same partition
    return (Math.Abs(tenantId.GetHashCode()) % PartitionCount).ToString();
}
```

## Error Handling and Resilience

### Retry Policies

```csharp
// Configure automatic retries
opts.OnException<SqlException>()
    .RetryWithCooldown(50.Milliseconds(), 100.Milliseconds(), 250.Milliseconds())
    .ThenMoveToErrorQueue();

opts.OnException<HttpRequestException>()
    .RetryWithExponentialBackoff(TimeSpan.FromSeconds(1), maxRetries: 5)
    .ThenMoveToErrorQueue();

// Business exceptions should not retry
opts.OnException<ValidationException>()
    .MoveToErrorQueue();
    
opts.OnException<BusinessRuleViolationException>()
    .MoveToErrorQueue();
```

### Dead Letter Queue Handling

```csharp
public class ErrorQueueHandler
{
    private readonly ILogger<ErrorQueueHandler> _logger;
    private readonly IMessageBus _messageBus;

    // Handle messages that failed processing
    public async Task HandleAsync(ErrorReport report)
    {
        _logger.LogError("Message {MessageId} of type {MessageType} failed {Attempts} times: {Error}",
            report.MessageId, report.MessageType, report.AttemptCount, report.ExceptionText);

        // Implement custom error handling logic
        if (report.AttemptCount > 5)
        {
            await SendToManualReview(report);
        }
        else if (IsTransientError(report.ExceptionText))
        {
            // Schedule for retry after investigation
            await ScheduleRetry(report, TimeSpan.FromHours(1));
        }
    }
}
```

## Testing Support

### In-Memory Testing

```csharp
[Test]
public async Task CreateCashier_ShouldPublishIntegrationEvent()
{
    // Arrange
    var testHarness = await AlbaHost.For<Program>(builder =>
    {
        builder.ConfigureServices(services =>
        {
            services.AddWolverine(opts =>
            {
                opts.Services.AddSingleton<MessagingMetricsStore>();
                opts.Policies.Add<FluentValidationPolicy>();
            });
        });
    });

    // Act
    var command = new CreateCashierCommand
    {
        Name = "Test Cashier",
        Email = "test@example.com",
        Currencies = new List<string> { "USD" }
    };

    var result = await testHarness.InvokeMessageAndWaitAsync(command);

    // Assert
    result.ShouldNotBeNull();
    
    // Verify integration event was published
    var publishedEvents = await testHarness.WaitForMessageToBeReceivedAsync<CashierCreated>();
    publishedEvents.Single().Message.Name.ShouldBe("Test Cashier");
}
```

### Integration Testing

```csharp
[Test]
public async Task ProcessPayment_EndToEnd_ShouldUpdateInvoiceStatus()
{
    // Arrange - use real Postgres and Kafka
    var host = await AlbaHost.For<Program>(builder =>
    {
        builder.UseTestcontainers(); // Spins up real infrastructure
    });

    // Act - simulate payment received
    await host.PublishMessageAsync(new PaymentReceived
    {
        InvoiceId = TestData.InvoiceId,
        Amount = 100.00m,
        Currency = "USD",
        PaymentMethod = "CreditCard"
    });

    // Assert - verify invoice was updated
    await host.WaitForMessageToBeReceivedAsync<InvoicePaid>(TimeSpan.FromSeconds(10));
    
    var invoice = await host.InvokeMessageAndWaitAsync(new GetInvoiceQuery 
    { 
        InvoiceId = TestData.InvoiceId 
    });
    
    invoice.Status.ShouldBe(InvoiceStatus.Paid);
}
```

## Value Delivered

### Development Velocity
- **80% reduction** in messaging boilerplate code
- **Type safety** eliminates runtime message routing errors
- **Automatic handler discovery** speeds up feature development
- **Rich testing support** enables test-driven development

### Reliability
- **Guaranteed delivery** with transactional outbox pattern
- **Automatic retries** handle transient failures
- **Dead letter queues** capture and analyze persistent failures
- **Transaction consistency** across database and messaging operations

### Observability
- **Complete message tracing** with OpenTelemetry integration
- **Performance metrics** for all message types
- **Error correlation** across distributed operations
- **Business event tracking** for audit and analytics

### Scalability
- **Multi-tenant partitioning** enables horizontal scaling
- **Parallel processing** with configurable concurrency
- **Circuit breakers** protect against cascade failures
- **Backpressure handling** prevents resource exhaustion

### Business Impact
- **Faster feature delivery** with reduced messaging complexity
- **Higher system reliability** with proven messaging patterns
- **Better customer experience** with consistent message processing
- **Reduced operational costs** with automated error handling