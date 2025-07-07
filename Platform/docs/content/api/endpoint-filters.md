---
title: Endpoint Filters
description: Learn about endpoint filters in the Operations platform, including built-in filters and creating custom filters.
---

# Endpoint Filters

This guide covers endpoint filters in the Operations platform, including built-in filters and creating custom filters.

## Overview

Endpoint filters in the Operations platform provide a way to execute code before and after minimal API endpoints. They offer a lightweight alternative to middleware for endpoint-specific logic.

## Built-in Filters

### LocalhostEndpointFilter

The platform includes a `LocalhostEndpointFilter` that restricts access to certain endpoints when running in production:

[!code-csharp[](~/samples/api/endpoint-filters/LocalhostEndpointFilter.cs?highlight=6,16-18)]

### Usage with Service Defaults

The service defaults automatically apply appropriate filters:

[!code-csharp[](~/samples/api/endpoint-filters/UsageWithServiceDefaults.cs?highlight=4,9)]

## Creating Custom Filters

### Basic Endpoint Filter

[!code-csharp[](~/samples/api/endpoint-filters/BasicLoggingEndpointFilter.cs?highlight=5,15,20,26)]

### Validation Filter

[!code-csharp[](~/samples/api/endpoint-filters/ValidationEndpointFilter.cs?highlight=11,22,25,33)]

### Authentication Filter

[!code-csharp[](~/samples/api/endpoint-filters/RequireRoleEndpointFilter.cs?highlight=6,17,20)]

### Rate Limiting Filter

[!code-csharp[](~/samples/api/endpoint-filters/RateLimitEndpointFilter.cs?highlight=10,20,23,33)]

## Filter Registration

### Global Registration

[!code-csharp[](~/samples/api/endpoint-filters/GlobalFilterRegistration.cs?highlight=5-8)]

### Endpoint-Specific Application

[!code-csharp[](~/samples/api/endpoint-filters/EndpointSpecificApplication.cs?highlight=7-8,11-13,17,21)]

### Group-Level Filters

[!code-csharp[](~/samples/api/endpoint-filters/GroupLevelFilters.cs?highlight=9-11,14,19)]

## Advanced Patterns

### Conditional Filters

[!code-csharp[](~/samples/api/endpoint-filters/ConditionalLoggingFilter.cs)]

### Filter with Dependencies

[!code-csharp[](~/samples/api/endpoint-filters/CacheEndpointFilter.cs)]

### Async Filter with Cancellation

[!code-csharp[](~/samples/api/endpoint-filters/TimeoutEndpointFilter.cs)]

## Filter Order and Pipeline

### Execution Order

Filters execute in the order they are added:

[!code-csharp[](~/samples/api/endpoint-filters/ExecutionOrder.cs)]

### Short-Circuiting

Filters can short-circuit the pipeline:

[!code-csharp[](~/samples/api/endpoint-filters/ShortCircuitFilter.cs)]

## Testing Endpoint Filters

### Unit Testing

[!code-csharp[](~/samples/api/endpoint-filters/ValidationFilter_InvalidModel_ReturnsValidationProblem.cs)]

### Integration Testing

[!code-csharp[](~/samples/api/endpoint-filters/CashierEndpoint_WithFilters_WorksCorrectly.cs)]

## Best Practices

1. **Single Responsibility**: Keep filters focused on specific concerns
2. **Performance**: Minimize overhead in frequently executed filters
3. **Error Handling**: Handle exceptions gracefully
4. **Dependency Injection**: Use DI for filter dependencies
5. **Testing**: Write comprehensive tests for custom filters
6. **Documentation**: Document filter behavior and usage
7. **Ordering**: Consider filter execution order carefully

## See Also

- [Service Defaults](../../content/architecture/service-defaults.md)
- [OpenAPI Documentation](openapi/overview.md)
- [gRPC Integration](grpc.md)
