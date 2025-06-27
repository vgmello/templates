---
title: Messaging Telemetry
description: Understand how messaging telemetry is integrated into your services for monitoring and observability.
---

# Messaging Telemetry

Messaging telemetry provides insights into the performance and behavior of your messaging infrastructure. The Platform integrates OpenTelemetry to automatically collect traces and metrics related to message production and consumption, enabling comprehensive monitoring and troubleshooting.

## Key Telemetry Aspects

### Distributed Tracing

Distributed tracing allows you to follow the journey of a message across different services, providing a complete view of its lifecycle. This is crucial for debugging issues in distributed systems.

[!code-csharp[](~/docs/samples/messaging/telemetry/MessageTracingExample.cs)]

### Metrics Collection

Metrics provide quantitative data about your messaging system, such as message rates, processing times, and error counts. This data can be used to create dashboards and alerts.

[!code-csharp[](~/docs/samples/messaging/telemetry/MessageMetricsExample.cs)]

## Configuration

Messaging telemetry is typically configured as part of your overall OpenTelemetry setup within the Service Defaults. You can customize the exporters and instrumentation as needed.

[!code-csharp[](~/docs/samples/messaging/telemetry/TelemetryConfiguration.cs)]

## See also

*   [OpenTelemetry Overview](../../opentelemetry/overview.md)
*   [Wolverine Messaging](../wolverine-integration.md)
