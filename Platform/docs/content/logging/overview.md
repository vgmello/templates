---
title: Logging
description: Structured logging with Serilog, OpenTelemetry integration, and dynamic log level configuration for comprehensive application observability.
ms.date: 01/27/2025
---

# Logging

The Platform provides comprehensive structured logging capabilities using Serilog with OpenTelemetry integration, automatic configuration from appsettings, and dynamic log level management. You get production-ready logging infrastructure that scales from development to enterprise environments.

## Quick start

Logging is automatically configured when you add Service Defaults:

This setup provides:

-   **Structured logging** with Serilog as the primary provider
-   **OpenTelemetry integration** for distributed tracing correlation
-   **Configuration-based** setup from appsettings.json
-   **Log context enrichment** with request correlation
-   **Two-stage initialization** for startup error capture

## Core logging features

### Structured logging with Serilog

Rich, queryable log data with semantic structure:

Structured logging benefits:

-   **Queryable properties** for log analysis
-   **Type-safe logging** with compile-time validation
-   **Rich context** capture without string concatenation
-   **Performance optimized** message templates

### Log context enrichment

Automatic enrichment with correlation and context data:

Context enrichment includes:

-   **Request correlation IDs** for tracing requests
-   **User identity** information when available
-   **Machine name** and environment details
-   **Custom properties** via log context scope

### OpenTelemetry integration

Seamless integration with distributed tracing:

OpenTelemetry integration provides:

-   **Trace correlation** linking logs to spans
-   **Activity ID** propagation across service boundaries
-   **Structured export** to observability platforms
-   **Consistent correlation** with metrics and traces

## Configuration management

### Configuration-driven setup

Configure logging behavior through appsettings:

Configuration capabilities:

-   **Log levels** per namespace and category
-   **Output sinks** configuration (Console, File, etc.)
-   **Enricher settings** and custom properties
-   **Filtering rules** for noise reduction

### Dynamic log level changes

Modify log levels at runtime without application restart:

Dynamic configuration features:

-   **Runtime adjustment** of log levels
-   **Hot reload** support for configuration changes
-   **Granular control** by namespace or category
-   **Temporary overrides** for debugging

### Environment-specific configuration

Different logging behaviors per environment:

Environment adaptations:

-   **Development** - Console output with detailed formatting
-   **Production** - JSON structured logs with minimal overhead
-   **Staging** - Enhanced logging for deployment validation
-   **Testing** - Suppressed noise with focused error capture

## Log sinks and outputs

### Console sink configuration

Optimized console output for different environments:

Console sink features:

-   **Human-readable format** in development
-   **JSON format** for container environments
-   **Color-coded output** for log level distinction
-   **Template customization** for specific needs

### File sink configuration

Persistent log storage with rotation and management:

File sink capabilities:

-   **Automatic rotation** by size or time
-   **Retention policies** for disk space management
-   **Structured or plain text** output formats
-   **Async writing** for performance

### External system integration

Send logs to monitoring and analysis platforms:

External integration options:

-   **Application Insights** for Azure environments
-   **Elasticsearch** for centralized log search
-   **Fluentd/Fluent Bit** for log forwarding
-   **Custom sinks** for proprietary systems

## Performance optimization

### Asynchronous logging

High-performance async logging for minimal application impact:

Async logging benefits:

-   **Non-blocking** log operations
-   **Buffered writes** for batching efficiency
-   **Backpressure handling** for high-volume scenarios
-   **Graceful shutdown** with log flushing

### Log filtering and sampling

Reduce logging overhead with intelligent filtering:

Filtering capabilities:

-   **Namespace-based** filtering for noise reduction
-   **Sampling rules** for high-frequency events
-   **Conditional logging** based on context
-   **Performance-critical** path optimization

### Memory and allocation optimization

Minimize memory pressure from logging operations:

Memory optimizations:

-   **Object pooling** for log event reuse
-   **String interning** for repeated values
-   **Minimal allocations** in hot paths
-   **GC pressure** reduction techniques

## Log analysis and monitoring

### Structured query capabilities

Query logs with structured properties:

Query capabilities include:

-   **Property-based filtering** on structured data
-   **Time range** queries for incident investigation
-   **Correlation** queries across request boundaries
-   **Aggregation** for metrics and trends

### Alert and notification setup

Automated alerting on log-based conditions:

Alerting features:

-   **Error threshold** monitoring
-   **Performance degradation** detection
-   **Business metric** alerting from logs
-   **Custom rule** configuration

### Dashboard and visualization

Create dashboards from structured log data:

Visualization capabilities:

-   **Real-time dashboards** from log streams
-   **Historical analysis** and trending
-   **Business intelligence** from application logs
-   **Custom metrics** derived from log data

## Security and compliance

### Sensitive data protection

Prevent logging of sensitive information:

Data protection features:

-   **Automatic scrubbing** of sensitive patterns
-   **Allowlist/blocklist** for properties
-   **Encryption** of sensitive log data
-   **Audit trail** for log access

### Compliance and retention

Meet regulatory requirements for log retention:

Compliance capabilities:

-   **Retention policies** per regulation requirements
-   **Immutable storage** for audit purposes
-   **Access controls** for log data
-   **Anonymization** for privacy compliance

## Testing and debugging

### Testing with logging

Verify logging behavior in unit and integration tests:

Testing approaches:

-   **Log assertion** in unit tests
-   **Sink verification** for output validation
-   **Performance testing** of logging overhead
-   **Integration testing** with real sinks

### Debug logging patterns

Effective debugging with structured logs:

Debugging techniques:

-   **Correlation tracking** across service calls
-   **State capture** at decision points
-   **Performance timing** for bottleneck identification
-   **Context preservation** through async operations

## Custom logging patterns

### Business event logging

Log business-relevant events for analytics:

Business event features:

-   **Domain event** capture for business intelligence
-   **User journey** tracking across interactions
-   **Performance metrics** for business operations
-   **Audit trail** for compliance and debugging

### Error handling integration

Comprehensive error logging with context:

Error logging includes:

-   **Exception details** with full stack traces
-   **Context preservation** from error boundaries
-   **Correlation** with user actions
-   **Aggregation** for error trending

### Performance logging

Monitor application performance through logs:

Performance logging covers:

-   **Operation timing** for key business processes
-   **Resource utilization** monitoring
-   **Bottleneck identification** through correlation
-   **Trend analysis** for capacity planning

## Best practices

-   **Use structured logging** with semantic properties over string interpolation
-   **Configure log levels** appropriately for each environment
-   **Implement async logging** for high-performance scenarios
-   **Protect sensitive data** from accidental logging
-   **Set up retention policies** early for compliance
-   **Monitor logging performance** itself for overhead
-   **Correlate logs** with traces and metrics
-   **Test logging** behavior as part of your pipeline

## Common scenarios

### Request correlation tracking

Track requests across service boundaries:

### Database operation logging

Monitor database performance and issues:

### Background job logging

Track background processing and errors:

## Troubleshooting

### Common logging issues

Diagnose and resolve logging problems:

### Performance tuning

Optimize logging performance for production:

### Configuration debugging

Debug logging configuration issues:

## Next steps

-   Learn about [Dynamic Log Levels](dynamic-log-levels.md) for runtime configuration
-   Explore [OpenTelemetry](../opentelemetry/overview.md) for distributed tracing
-   Understand [Health Checks](../healthchecks/overview.md) logging integration
-   Review [Extensions](../extensions/overview.md) for logging utilities

## Additional resources

-   [Serilog Documentation](https://serilog.net/)
-   [ASP.NET Core Logging](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/)
-   [OpenTelemetry Logging](https://opentelemetry.io/docs/languages/net/logs/)
-   [Structured Logging Best Practices](https://serilog.net/wiki/structured-data)
