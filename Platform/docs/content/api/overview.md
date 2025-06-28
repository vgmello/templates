---
title: API Development
description: Build robust APIs with Platform's integrated OpenAPI documentation, gRPC support, and standardized middleware pipeline.
---

# API Development

The Platform provides comprehensive API development capabilities, combining REST and gRPC services with automatic documentation generation, built-in security, and standardized middleware. You get production-ready APIs with minimal configuration.

## Quick start

Set up a complete API service with just a few lines:

[!code-csharp[](~/samples/api/ApiSetup.cs?highlight=6-7)]

This setup provides:

-   **REST API controllers** with automatic model binding
-   **gRPC services** with automatic discovery
-   **OpenAPI documentation** with XML comment integration
-   **Authentication and authorization** ready to configure
-   **Problem details** for consistent error responses
-   **HTTP logging** for debugging and monitoring

## REST API features

### Automatic OpenAPI generation

Your controllers automatically generate comprehensive OpenAPI documentation:

[!code-csharp[](~/samples/api/OpenApiController.cs)]

The Platform automatically generates:

-   Request/response schemas
-   Parameter documentation
-   Status code responses
-   Example values
-   Security requirements

> [!TIP]
> Use XML documentation comments to enrich your OpenAPI specification with detailed descriptions and examples.

### Problem details integration

Consistent error responses using RFC 7807 Problem Details:

[!code-csharp[](~/samples/api/ProblemDetailsExample.cs)]

Problem details provide:

-   Standardized error format
-   Machine-readable error types
-   Human-readable error messages
-   Additional error context

### HTTP logging

Built-in request/response logging for debugging and monitoring:

[!code-csharp[](~/samples/api/HttpLoggingConfiguration.cs)]

HTTP logging captures:

-   Request headers and body
-   Response status and headers
-   Request duration
-   Sensitive data redaction

## gRPC integration

### Automatic service discovery

gRPC services are automatically discovered and registered:

[!code-csharp[](~/samples/api/GrpcServiceExample.cs)]

The Platform automatically:

-   Discovers services inheriting from generated base classes
-   Maps services to gRPC endpoints
-   Enables gRPC-Web for browser clients
-   Provides reflection services in development

### gRPC-Web support

Enable gRPC services to be called from web browsers:

[!code-csharp[](~/samples/api/GrpcWebConfiguration.cs)]

gRPC-Web provides:

-   Browser compatibility for gRPC services
-   Streaming support where possible
-   Automatic protocol negotiation
-   CORS support for cross-origin requests

### Development tools

Enhanced development experience with gRPC reflection:

[!code-csharp[](~/samples/api/GrpcReflectionSetup.cs)]

gRPC reflection enables:

-   Service discovery for client tools
-   Interactive testing with gRPC clients
-   Dynamic schema exploration
-   Protocol buffer introspection

## Environment-specific behavior

### Development environment

Enhanced development experience:

[!code-csharp[](~/samples/api/DevelopmentConfiguration.cs)]

Development features:

-   Interactive OpenAPI documentation
-   gRPC reflection services
-   Detailed error responses
-   OpenAPI caching middleware

### Production environment

Production-optimized configuration:

[!code-csharp[](~/samples/api/ProductionConfiguration.cs)]

Production features:

-   Global exception handling
-   Minimal error disclosure
-   Performance optimizations

## OpenAPI documentation

### XML documentation integration

Automatically generate rich API documentation:

[!code-csharp[](~/samples/api/XmlDocumentationExample.cs)]

XML documentation provides:

-   Parameter descriptions
-   Response type documentation
-   Example values
-   Operation summaries

### Scalar UI integration

Beautiful, interactive API documentation:

[!code-csharp[](~/samples/api/ScalarIntegration.cs)]

Scalar provides:

-   Modern, responsive UI
-   Interactive API testing
-   Code generation samples
-   Authentication testing

### Custom transformers

Extend OpenAPI generation with custom transformers:

[!code-csharp[](~/samples/api/CustomTransformers.cs)]

Transformers enable:

-   Custom schema modifications
-   Additional metadata injection
-   Response type enrichment
-   Documentation enhancement

## Performance considerations

### Kestrel optimization

Optimized server configuration:

[!code-csharp[](~/samples/api/KestrelOptimization.cs)]

Performance optimizations:

-   Disabled server header for security
-   Optimized connection limits
-   Efficient request processing
-   Memory usage optimization

### gRPC performance

High-performance gRPC configuration:

[!code-csharp[](~/samples/api/GrpcPerformance.cs)]

gRPC optimizations:

-   Binary serialization
-   HTTP/2 multiplexing
-   Streaming support
-   Connection pooling

## Testing strategies

### Integration testing

Test complete API endpoints:

[!code-csharp[](~/samples/api/IntegrationTesting.cs)]

### gRPC testing

Test gRPC services with specialized clients:

[!code-csharp[](~/samples/api/GrpcTesting.cs)]

### OpenAPI validation

Ensure API contracts match implementation:

[!code-csharp[](~/samples/api/OpenApiValidation.cs)]

## Best practices

-   **Use XML documentation** for all public API methods
-   **Follow REST conventions** for HTTP endpoints
-   **Implement proper error handling** with Problem Details
-   **Secure endpoints** with appropriate authorization
-   **Version your APIs** for backward compatibility
-   **Test API contracts** with integration tests
-   **Monitor API performance** with telemetry

## Common scenarios

### File upload endpoint

Handle file uploads with validation:

[!code-csharp[](~/samples/api/FileUploadEndpoint.cs)]

### Paginated results

Return paginated data efficiently:

[!code-csharp[](~/samples/api/PaginatedResults.cs)]

### Background job triggers

Trigger background jobs from API endpoints:

[!code-csharp[](~/samples/api/BackgroundJobTrigger.cs)]

## Next steps

-   Learn about [OpenAPI documentation](openapi/overview.md) in detail
-   Explore [gRPC integration](grpc.md) patterns
-   Understand [Endpoint filters](endpoint-filters.md) for cross-cutting concerns
-   Review [Database integration](../database-integration/overview.md) for data access

## Additional resources

-   [ASP.NET Core Web APIs](https://learn.microsoft.com/en-us/aspnet/core/web-api/)
-   [gRPC for .NET](https://learn.microsoft.com/en-us/aspnet/core/grpc/)
-   [OpenAPI Specification](https://spec.openapis.org/oas/v3.1.0)
-   [Problem Details RFC](https://tools.ietf.org/html/rfc7807)
