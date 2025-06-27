# Platform Operations Service Documentation

Welcome to the Platform Operations Service documentation.

This service provides core platform functionality and shared extensions for the microservices architecture.

## Getting Started

- [API Reference](api/Operations.html)
- [Architecture Overview](content/architecture.md)

## Key Components

### Extensions
- **Operations.Extensions** - Core utilities and extension methods
- **Operations.Extensions.Abstractions** - Common interfaces and abstractions
- **Operations.Extensions.SourceGenerators** - Source generators for database commands

### Service Defaults
- **Operations.ServiceDefaults** - Common service configuration and middleware
- **Operations.ServiceDefaults.Api** - API-specific extensions and OpenAPI configuration

## Architecture

The Platform Operations Service follows Domain-Driven Design principles and provides:

- Shared infrastructure patterns
- Common messaging abstractions
- Database command source generation
- OpenTelemetry integration
- Health check infrastructure