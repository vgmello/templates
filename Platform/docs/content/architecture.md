# Platform Architecture

The Platform Operations Service provides foundational infrastructure for the microservices architecture.

## Overview

The platform consists of several key components:

### Core Extensions (`Operations.Extensions`)
- **Result Pattern**: Standardized error handling
- **Messaging Extensions**: Common messaging patterns
- **Dapper Extensions**: Database access utilities

### Abstractions (`Operations.Extensions.Abstractions`)
- **ICommand/IQuery**: CQRS interfaces
- **IDbParamsProvider**: Database parameter providers
- **Attributes**: Metadata for source generation

### Source Generators (`Operations.Extensions.SourceGenerators`)
- **DbCommand Generation**: Automatic database command handler generation
- **Parameter Mapping**: Type-safe parameter mapping
- **Performance Optimization**: Compile-time code generation

### Service Defaults (`Operations.ServiceDefaults`)
- **Health Checks**: Standardized health check patterns
- **Logging**: Structured logging setup
- **OpenTelemetry**: Distributed tracing and metrics
- **Messaging**: Wolverine integration and middleware

### API Defaults (`Operations.ServiceDefaults.Api`)
- **OpenAPI**: Comprehensive API documentation
- **gRPC**: Service registration and extensions
- **Validation**: FluentValidation integration
- **Error Handling**: Standardized error responses

## Design Principles

1. **Consistency**: Common patterns across all services
2. **Performance**: Source generation for zero-allocation paths
3. **Observability**: Built-in telemetry and monitoring
4. **Maintainability**: Clear separation of concerns
5. **Developer Experience**: Rich tooling and documentation