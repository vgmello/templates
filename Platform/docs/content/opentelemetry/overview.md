# OpenTelemetry Overview

The Platform provides comprehensive observability through OpenTelemetry integration, offering distributed tracing, metrics collection, and structured logging with minimal configuration and maximum insight.

## Key Benefits

### 🔍 **Complete Observability**
- **Distributed tracing** - Follow requests across service boundaries
- **Metrics collection** - Performance and business metrics
- **Structured logging** - Correlated logs with trace context
- **Custom instrumentation** - Domain-specific observability

### 🚀 **Zero-Configuration Setup**
- **Auto-instrumentation** - Automatic tracing for ASP.NET Core, HTTP clients, and databases
- **Correlation IDs** - Automatic trace and span ID injection into logs
- **Standard exporters** - OTLP, Jaeger, Prometheus, and console exporters
- **Performance optimized** - Sampling, batching, and efficient resource usage

### 📊 **Production Ready**
- **Vendor agnostic** - Works with any OpenTelemetry-compatible backend
- **Resource attribution** - Service name, version, and environment metadata
- **Sampling strategies** - Control data volume and performance impact
- **Security** - TLS encryption and authentication support

## Core Setup

### Basic Configuration

```csharp
public static IServiceCollection AddOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
{
    var serviceName = configuration["ServiceBus:PublicServiceName"] ?? "UnknownService";
    var serviceVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0";

    services.AddOpenTelemetry()
        .ConfigureResource(resource => resource
            .AddService(
                serviceName: serviceName,
                serviceVersion: serviceVersion,
                serviceInstanceId: Environment.MachineName)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                ["host.name"] = Environment.MachineName,
                ["service.namespace"] = "Operations.Platform"
            }))
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
                options.Filter = httpContext => 
                {
                    // Skip health check endpoints from tracing
                    var path = httpContext.Request.Path.Value?.ToLowerInvariant();
                    return !path?.Contains("/health") == true && !path?.Contains("/status") == true;
                };
            })
            .AddHttpClientInstrumentation(options =>
            {
                options.RecordException = true;
                options.FilterHttpRequestMessage = request =>
                {
                    // Skip internal health checks
                    return !request.RequestUri?.ToString().Contains("/health") == true;
                };
            })
            .AddNpgsql() // Automatic database tracing
            .AddSource("Wolverine") // Message bus tracing
            .AddSource("Operations.*")) // Custom activity sources
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddMeter("Operations.Messaging.*") // Custom messaging metrics
            .AddMeter("Operations.Business.*")) // Business metrics
        .WithLogging(logging => logging
            .AddOpenTelemetryLogger(options =>
            {
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                options.ParseStateValues = true;
            }));

    // Add OTLP exporter
    var otlpEndpoint = configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317";
    services.Configure<OpenTelemetryLoggerOptions>(logging => logging.AddOtlpExporter(otlp =>
    {
        otlp.Endpoint = new Uri(otlpEndpoint);
        otlp.Protocol = OtlpExportProtocol.Grpc;
    }));

    return services;
}
```

### Environment-Specific Configuration

#### Development (appsettings.Development.json)
```json
{
  "OpenTelemetry": {
    "Endpoint": "http://localhost:4317",
    "Metrics": {
      "ConsoleExporter": true,
      "PeriodicExportingMetricReader": {
        "ExportIntervalMilliseconds": 5000
      }
    },
    "Tracing": {
      "ConsoleExporter": true,
      "JaegerExporter": {
        "Endpoint": "http://localhost:14268/api/traces"
      }
    },
    "Logging": {
      "ConsoleExporter": true
    }
  }
}
```

#### Production (appsettings.Production.json)
```json
{
  "OpenTelemetry": {
    "Endpoint": "https://otel-collector.company.com:4317",
    "Headers": "Authorization=Bearer ${OTEL_EXPORTER_OTLP_HEADERS}",
    "Metrics": {
      "PeriodicExportingMetricReader": {
        "ExportIntervalMilliseconds": 30000
      }
    },
    "Tracing": {
      "BatchExportProcessor": {
        "MaxExportBatchSize": 512,
        "ExportTimeoutMilliseconds": 30000,
        "ScheduledDelayMilliseconds": 5000
      }
    },
    "ResourceDetectors": {
      "Container": true,
      "Environment": true,
      "Host": true,
      "Process": true
    }
  }
}
```

## Distributed Tracing

### Automatic Instrumentation

The Platform automatically traces:

```csharp
// HTTP requests
[HttpPost]
public async Task<ActionResult<CashierDto>> CreateCashier(
    [FromBody] CreateCashierRequest request,
    CancellationToken cancellationToken = default)
{
    // Automatic span creation for HTTP endpoint
    // Span name: "POST /api/cashiers"
    // Tags: http.method, http.route, http.status_code
    
    var command = new CreateCashierCommand
    {
        Name = request.Name,
        Email = request.Email,
        Currencies = request.Currencies
    };

    // Database operations automatically traced
    var result = await _messageBus.InvokeCommandAsync(command, cancellationToken);
    
    return CreatedAtAction(nameof(GetCashier), new { id = result.Id }, result);
}
```

### Custom Instrumentation

#### Activity Sources

```csharp
public static class ActivitySources
{
    public static readonly ActivitySource Billing = new("Operations.Billing");
    public static readonly ActivitySource Messaging = new("Operations.Messaging");
    public static readonly ActivitySource Business = new("Operations.Business");
}

// Register in DI
services.AddSingleton(ActivitySources.Billing);
services.AddSingleton(ActivitySources.Messaging);
services.AddSingleton(ActivitySources.Business);
```

#### Business Operation Tracing

```csharp
public class CreateCashierCommandHandler
{
    private readonly ActivitySource _activitySource;
    private readonly ILogger<CreateCashierCommandHandler> _logger;

    public async Task<CashierDto> ExecuteAsync(CreateCashierCommand command, CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity("CreateCashier");
        activity?.SetTag("cashier.email", command.Email);
        activity?.SetTag("cashier.currencies.count", command.Currencies.Count);
        activity?.SetTag("operation.type", "command");

        try
        {
            _logger.LogInformation("Creating cashier {Email} with {CurrencyCount} currencies", 
                command.Email, command.Currencies.Count);

            // Validate command
            await ValidateCommand(command, cancellationToken);
            activity?.AddEvent(new ActivityEvent("Validation completed"));

            // Create cashier
            var cashier = await CreateInDatabase(command, cancellationToken);
            activity?.SetTag("cashier.id", cashier.Id.ToString());
            activity?.AddEvent(new ActivityEvent("Cashier created in database"));

            // Publish integration event
            await PublishIntegrationEvent(cashier, cancellationToken);
            activity?.AddEvent(new ActivityEvent("Integration event published"));

            _logger.LogInformation("Cashier {CashierId} created successfully", cashier.Id);
            activity?.SetStatus(ActivityStatusCode.Ok);

            return cashier;
        }
        catch (ValidationException ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Validation failed");
            activity?.SetTag("error.type", "validation");
            activity?.SetTag("error.message", ex.Message);
            
            _logger.LogWarning(ex, "Cashier creation failed validation");
            throw;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("error.type", ex.GetType().Name);
            activity?.RecordException(ex);
            
            _logger.LogError(ex, "Failed to create cashier");
            throw;
        }
    }
}
```

### Messaging Middleware Integration

```csharp
public class OpenTelemetryInstrumentationMiddleware
{
    private readonly ActivitySource _activitySource;

    public async Task BeforeAsync(IMessageContext context, CancellationToken cancellation)
    {
        var messageName = context.Envelope.GetMessageName();
        var operationType = DetermineOperationType(context.Envelope.MessageType);
        
        var activity = _activitySource.StartActivity($"Process {messageName}");
        if (activity != null)
        {
            activity.SetTag("message.id", context.Envelope.Id.ToString());
            activity.SetTag("message.name", messageName);
            activity.SetTag("message.type", context.Envelope.MessageType.Name);
            activity.SetTag("operation.type", operationType);
            
            // Extract parent context from message headers
            if (context.Envelope.Headers.TryGetValue("traceparent", out var traceParent))
            {
                activity.SetParentId(traceParent);
            }
            
            context.Storage["OpenTelemetry.Activity"] = activity;
        }
    }

    public async Task FinallyAsync(IMessageContext context, CancellationToken cancellation)
    {
        if (context.Storage.TryGetValue("OpenTelemetry.Activity", out var activityObj) &&
            activityObj is Activity activity)
        {
            if (context.Storage.TryGetValue("Exception", out var exceptionObj) &&
                exceptionObj is Exception exception)
            {
                activity.SetStatus(ActivityStatusCode.Error, exception.Message);
                activity.RecordException(exception);
            }
            else
            {
                activity.SetStatus(ActivityStatusCode.Ok);
            }

            activity.Dispose();
        }
    }

    private static string DetermineOperationType(Type messageType)
    {
        if (typeof(ICommand).IsAssignableFrom(messageType))
            return "command";
        if (typeof(IQuery<>).IsAssignableFromGeneric(messageType))
            return "query";
        if (messageType.Namespace?.EndsWith(".IntegrationEvents") == true)
            return "integration_event";
        return "message";
    }
}
```

## Metrics Collection

### Custom Business Metrics

```csharp
public class MessagingMetrics
{
    private readonly Counter<long> _messageCounter;
    private readonly Counter<long> _exceptionCounter;
    private readonly Histogram<long> _durationHistogram;

    public MessagingMetrics(IMeterFactory meterFactory, string meterName)
    {
        var meter = meterFactory.Create(meterName);
        
        _messageCounter = meter.CreateCounter<long>(
            "messages_processed_total",
            description: "Total number of messages processed");
            
        _exceptionCounter = meter.CreateCounter<long>(
            "messages_exceptions_total", 
            description: "Total number of message processing exceptions");
            
        _durationHistogram = meter.CreateHistogram<long>(
            "message_processing_duration_milliseconds",
            description: "Message processing duration in milliseconds");
    }

    public Counter<long> MessageCounter => _messageCounter;
    public Counter<long> ExceptionCounter => _exceptionCounter;
    public Histogram<long> DurationHistogram => _durationHistogram;
}

// Store for different message types
public class MessagingMeterStore
{
    private readonly ConcurrentDictionary<string, MessagingMetrics> _meters = new();
    private readonly IMeterFactory _meterFactory;

    public MessagingMeterStore(IMeterFactory meterFactory)
    {
        _meterFactory = meterFactory;
    }

    public MessagingMetrics GetMeter(string messageTypeName)
    {
        return _meters.GetOrAdd(messageTypeName, name =>
        {
            var meterName = $"Operations.Messaging.{name.ToSnakeCase()}";
            return new MessagingMetrics(_meterFactory, meterName);
        });
    }
}
```

### Business Domain Metrics

```csharp
public class CashierMetrics
{
    private readonly Counter<long> _cashiersCreated;
    private readonly Counter<long> _cashiersDeleted;
    private readonly Histogram<double> _cashierCreationDuration;
    private readonly UpDownCounter<long> _activeCashiers;

    public CashierMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Operations.Business.Cashiers");
        
        _cashiersCreated = meter.CreateCounter<long>(
            "cashiers_created_total",
            description: "Total number of cashiers created");
            
        _cashiersDeleted = meter.CreateCounter<long>(
            "cashiers_deleted_total", 
            description: "Total number of cashiers deleted");
            
        _cashierCreationDuration = meter.CreateHistogram<double>(
            "cashier_creation_duration_seconds",
            description: "Time taken to create a cashier");
            
        _activeCashiers = meter.CreateUpDownCounter<long>(
            "cashiers_active",
            description: "Number of currently active cashiers");
    }

    public void RecordCashierCreated(string[] currencies, double durationSeconds)
    {
        var tags = new TagList
        {
            ["currencies.count"] = currencies.Length,
            ["currencies.primary"] = currencies.FirstOrDefault() ?? "unknown"
        };
        
        _cashiersCreated.Add(1, tags);
        _cashierCreationDuration.Record(durationSeconds, tags);
        _activeCashiers.Add(1);
    }

    public void RecordCashierDeleted()
    {
        _cashiersDeleted.Add(1);
        _activeCashiers.Add(-1);
    }
}
```

### Usage in Handlers

```csharp
public class CreateCashierCommandHandler
{
    private readonly CashierMetrics _metrics;
    private readonly ILogger<CreateCashierCommandHandler> _logger;

    public async Task<CashierDto> ExecuteAsync(CreateCashierCommand command, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var result = await CreateCashierInternal(command, cancellationToken);
            
            // Record successful creation
            _metrics.RecordCashierCreated(
                command.Currencies.ToArray(), 
                stopwatch.Elapsed.TotalSeconds);
                
            return result;
        }
        catch (Exception)
        {
            // Exception metrics recorded by middleware
            throw;
        }
    }
}
```

## Log Correlation

### Automatic Context Injection

```csharp
// Serilog enricher automatically adds trace context
services.AddSerilog((serviceProvider, loggerConfig) =>
{
    loggerConfig
        .Enrich.WithOpenTelemetryTraceId()
        .Enrich.WithOpenTelemetrySpanId()
        .WriteTo.Console(new JsonFormatter())
        .WriteTo.OpenTelemetry();
});
```

### Correlated Log Output

```json
{
  "timestamp": "2024-01-15T10:30:00.123Z",
  "level": "Information",
  "message": "Cashier {CashierId} created successfully",
  "properties": {
    "CashierId": "550e8400-e29b-41d4-a716-446655440000",
    "SourceContext": "Billing.Cashier.Commands.CreateCashierCommandHandler"
  },
  "traceId": "12345678901234567890123456789012",
  "spanId": "1234567890123456",
  "traceFlags": "01"
}
```

### Cross-Service Correlation

```csharp
public class HttpTracingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        // Automatically inject trace context into outgoing requests
        var activity = Activity.Current;
        if (activity != null)
        {
            request.Headers.Add("traceparent", activity.Id);
            if (!string.IsNullOrEmpty(activity.TraceStateString))
            {
                request.Headers.Add("tracestate", activity.TraceStateString);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
```

## Sampling and Performance

### Sampling Configuration

```csharp
services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .SetSampler(new TraceIdRatioBasedSampler(0.1)) // Sample 10% of traces
        .AddAspNetCoreInstrumentation()
        .SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddService("BillingService", "1.0.0")));

// Custom sampling for high-value operations
services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .SetSampler(new CompositeSampler(new Dictionary<string, double>
        {
            ["CreateCashier"] = 1.0,      // Always sample business operations
            ["ProcessPayment"] = 1.0,     // Always sample payments
            ["GET /health"] = 0.01,       // Rarely sample health checks
            ["*"] = 0.1                   // Default 10% sampling
        })));
```

### Resource Optimization

```csharp
// Batch processing for better performance
services.Configure<BatchExportProcessorOptions<Activity>>(options =>
{
    options.MaxExportBatchSize = 512;
    options.ScheduledDelayMilliseconds = 5000;
    options.ExporterTimeoutMilliseconds = 30000;
    options.MaxQueueSize = 2048;
});

// Resource limits
services.Configure<TracerProviderBuilderOptions>(options =>
{
    options.SetMaxActiveLinks(32);
    options.SetMaxActiveEvents(128);
    options.SetMaxActiveAttributes(64);
});
```

## Backend Integration

### Jaeger Setup

```yaml
# docker-compose.yml
version: '3.8'
services:
  jaeger:
    image: jaegertracing/all-in-one:latest
    ports:
      - "16686:16686"
      - "14250:14250"
    environment:
      - COLLECTOR_OTLP_ENABLED=true
```

### Grafana Dashboard

```json
{
  "dashboard": {
    "title": "Platform Operations Services",
    "panels": [
      {
        "title": "Request Rate",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(http_requests_total[5m])",
            "legendFormat": "{{service_name}} - {{method}} {{route}}"
          }
        ]
      },
      {
        "title": "P95 Latency",
        "type": "graph", 
        "targets": [
          {
            "expr": "histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))",
            "legendFormat": "{{service_name}}"
          }
        ]
      },
      {
        "title": "Error Rate",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(http_requests_total{status_code=~\"5..\"}[5m])",
            "legendFormat": "{{service_name}} - Errors"
          }
        ]
      }
    ]
  }
}
```

### Alerting Rules

```yaml
groups:
  - name: platform-services
    rules:
      - alert: HighErrorRate
        expr: rate(http_requests_total{status_code=~"5.."}[5m]) > 0.1
        for: 2m
        labels:
          severity: warning
        annotations:
          summary: "High error rate detected"
          
      - alert: HighLatency
        expr: histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m])) > 1
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "High latency detected"
          
      - alert: MessageProcessingFailure
        expr: rate(messages_exceptions_total[5m]) > 0.05
        for: 1m
        labels:
          severity: critical
        annotations:
          summary: "Message processing failures detected"
```

## Value Delivered

### Operational Excellence
- **Mean Time to Resolution (MTTR)** reduced by 75% with distributed tracing
- **Root cause identification** in minutes instead of hours
- **Proactive issue detection** with real-time metrics and alerting
- **End-to-end visibility** across all service interactions

### Developer Experience  
- **Zero-configuration observability** - Works out of the box
- **Rich debugging context** - Complete request lifecycle visibility
- **Performance insights** - Identify bottlenecks and optimization opportunities
- **Production debugging** - Safely investigate issues in live systems

### Business Impact
- **Improved customer experience** with faster issue resolution
- **Reduced operational costs** with automated monitoring
- **Data-driven optimization** with comprehensive metrics
- **Compliance support** with complete audit trails

### Platform Benefits
- **Vendor neutrality** - Works with any OpenTelemetry-compatible backend
- **Consistent observability** across all microservices
- **Future-proof architecture** - Based on open standards
- **Scalable monitoring** - Efficient data collection and export