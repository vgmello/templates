# OpenTelemetry Setup and Configuration

This guide covers the setup and configuration of OpenTelemetry in the Operations platform.

## Overview

The Operations platform provides comprehensive OpenTelemetry support through the `Operations.ServiceDefaults.OpenTelemetry` package, enabling distributed tracing, metrics collection, and observability across all services.

## Basic Setup

### Service Registration

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add OpenTelemetry with default configuration
builder.AddServiceDefaults();

var app = builder.Build();

// Map default observability endpoints
app.MapDefaultEndpoints();

app.Run();
```

### Manual Configuration

For more control over OpenTelemetry configuration:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddGrpcClientInstrumentation())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation());
```

## Configuration Options

### Environment Variables

- `OTEL_SERVICE_NAME`: Service name for telemetry
- `OTEL_SERVICE_VERSION`: Service version
- `OTEL_RESOURCE_ATTRIBUTES`: Additional resource attributes
- `OTEL_EXPORTER_OTLP_ENDPOINT`: OpenTelemetry Collector endpoint

### appsettings.json

```json
{
  "OpenTelemetry": {
    "ServiceName": "billing-api",
    "ServiceVersion": "1.0.0",
    "Otlp": {
      "Endpoint": "http://localhost:4317"
    }
  }
}
```

## Instrumentation

### Automatic Instrumentation

The platform automatically instruments:
- ASP.NET Core requests
- HTTP client calls
- gRPC client/server calls
- Database operations (via Dapper extensions)
- Messaging operations (via Wolverine)

### Custom Instrumentation

```csharp
// Custom activity source
private static readonly ActivitySource ActivitySource = new("Billing.Api");

// Creating custom spans
using var activity = ActivitySource.StartActivity("ProcessPayment");
activity?.SetTag("payment.id", paymentId);
activity?.SetTag("payment.amount", amount);
```

## Exporters

### OTLP Exporter (Default)

Exports to OpenTelemetry Collector:

```csharp
builder.Services.Configure<OpenTelemetryLoggerOptions>(logging => logging
    .AddOtlpExporter());
```

### Console Exporter (Development)

For local development:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddConsoleExporter())
    .WithMetrics(metrics => metrics.AddConsoleExporter());
```

## Best Practices

1. **Service Naming**: Use consistent service names across environments
2. **Resource Attributes**: Include deployment environment, version, and region
3. **Sampling**: Configure appropriate sampling rates for production
4. **Custom Metrics**: Create business-specific metrics for monitoring
5. **Error Handling**: Ensure telemetry doesn't impact application performance

## Troubleshooting

### Common Issues

- **Missing traces**: Verify OTLP endpoint configuration
- **High overhead**: Adjust sampling rates
- **Network errors**: Check collector connectivity
- **Missing attributes**: Verify resource configuration

### Debugging

Enable debug logging for OpenTelemetry:

```json
{
  "Logging": {
    "LogLevel": {
      "OpenTelemetry": "Debug"
    }
  }
}
```

## See Also

- [OpenTelemetry Overview](overview.md)
- [Health Checks Setup](../healthchecks/setup.md)
- [Logging Configuration](../logging/overview.md)