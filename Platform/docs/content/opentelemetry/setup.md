---
title: OpenTelemetry Setup Extensions
description: Learn how to configure OpenTelemetry for comprehensive observability in your services, including tracing, metrics, and logging.
---

# OpenTelemetry Setup Extensions

The `OpenTelemetrySetupExtensions` class provides a set of extension methods to easily integrate OpenTelemetry into your ASP.NET Core applications. OpenTelemetry is a vendor-neutral open-source observability framework that enables you to collect and export telemetry data (traces, metrics, and logs) from your services.

## Key Features

### AddOpenTelemetry

This extension method configures OpenTelemetry for your application, setting up tracing, metrics, and logging. It provides sensible defaults and allows for customization through configuration.

#### Tracing Configuration

-   **ActivitySource**: A new `ActivitySource` is created and registered, using the application name or a configurable name (`OpenTelemetry:ActivitySourceName`). This source is used to create custom spans for your application logic.
-   **AspNetCoreInstrumentation**: Automatically instruments ASP.NET Core requests, capturing details about incoming HTTP requests. It includes a filter to exclude health check endpoints from tracing.
-   **HttpClientInstrumentation**: Instruments outgoing HTTP requests made with `HttpClient`, allowing you to trace calls to external services. It includes a filter to exclude certain internal paths.
-   **Wolverine Instrumentation**: Adds instrumentation for Wolverine, ensuring that message processing is included in your traces.

#### Metrics Configuration

-   **Meter**: A new `Meter` is created and registered for messaging metrics, using the application name or a configurable name (`OpenTelemetry:MessagingMeterName`).
-   **AspNetCoreInstrumentation**: Instruments ASP.NET Core metrics, such as request duration and counts.
-   **HttpClientInstrumentation**: Instruments `HttpClient` metrics, such as outgoing request duration and counts.
-   **RuntimeInstrumentation**: Collects metrics from the .NET runtime, including CPU usage, memory, and garbage collection.
-   **Wolverine Metrics**: Integrates with Wolverine's internal metrics.
-   **Messaging Metrics**: Integrates with the custom `MessagingMeterStore` for application-specific messaging metrics.

#### Logging Configuration

-   **OpenTelemetry Logging**: Configures logging to be exported via OpenTelemetry, including formatted messages and scopes.

## Usage example

To add OpenTelemetry to your application, call `AddOpenTelemetry` on your `IHostApplicationBuilder`:

[!code-csharp[](~/samples/opentelemetry/SetupProgram.cs?highlight=3)]

## Configuration

You can configure OpenTelemetry settings in your `appsettings.json` file:

```json
{
  "OpenTelemetry": {
    "ActivitySourceName": "MyApplication.Activities",
    "MessagingMeterName": "MyApplication.MessagingMetrics"
  }
}
```

## See also

- [Messaging Meter Store](../messaging/telemetry/messaging-meter-store.md)
- [Messaging Metrics](../messaging/telemetry/messaging-metrics.md)
