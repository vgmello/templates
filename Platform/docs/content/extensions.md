# Platform extensions

Platform extensions provide comprehensive infrastructure for .NET microservices development. You get production-ready observability, messaging, database integration, and API capabilities through standardized extension methods that follow .NET configuration patterns.

:::moniker range=">= operations-1.0"

## Concept

Platform extensions eliminate infrastructure setup complexity by providing opinionated defaults for common microservice concerns. Instead of configuring dozens of services manually, you add extension methods that handle logging, health checks, messaging, and API development with consistent patterns across your entire system.

The extensions follow these principles:

- **Configuration over code** - Settings drive behavior, not hardcoded values
- **Composition-friendly** - Extensions work together seamlessly
- **Environment-aware** - Different behavior for development vs production
- **Observable by default** - All components emit telemetry automatically

## Example

Here's a complete microservice setup using Platform extensions:

:::code language="csharp" source="~/samples/extensions/CompleteService.cs" highlight="3-4,8-10":::

This example provides:
- Structured logging with Serilog
- OpenTelemetry metrics and tracing  
- Health checks with Kubernetes endpoints
- Wolverine messaging with PostgreSQL
- OpenAPI documentation with Scalar UI
- gRPC services with automatic discovery

> [!TIP]
> Start with `AddServiceDefaults()` for any service type, then add `AddApiServiceDefaults()` for web APIs or background processing capabilities.

## Core service extensions

Platform extensions are organized into logical groups based on functionality.

### AddServiceDefaults

Configures essential service infrastructure with production-ready defaults:

:::code language="csharp" source="~/samples/extensions/ServiceDefaults.cs" id="basic_setup":::

**Key features:**
- **Comprehensive setup** - Logging, OpenTelemetry, Wolverine messaging, validators, and health checks
- **Production ready** - Resilience patterns for HTTP clients and error handling
- **Service discovery** - Automatic registration with .NET Aspire
- **Extensible** - Customization options for specific requirements

> [!NOTE]
> `AddServiceDefaults()` should be the first extension called in any Platform-based service.

### AddValidators

Automatically discovers and registers FluentValidation validators:

:::code language="csharp" source="~/samples/extensions/Validators.cs" id="validator_setup":::

**Key features:**
- **Auto-discovery** - Scans assemblies for validator types automatically
- **Domain boundaries** - Uses `DomainAssemblyAttribute` for cross-service validation
- **Performance optimized** - Single-pass registration with assembly caching
- **Consistent patterns** - Uniform validation across all services

### RunAsync extension

Provides enhanced application lifecycle management:

:::code language="csharp" source="~/samples/extensions/EnhancedRunner.cs" id="run_async":::

**Key features:**
- **Wolverine integration** - Command-line operations (check-env, codegen, db-apply)
- **Graceful shutdown** - Proper resource cleanup and connection management
- **Exception handling** - Centralized error handling with observability
- **Development tools** - Code generation and environment validation

## API extensions

API extensions provide opinionated defaults for REST and gRPC services with comprehensive documentation and security.

### AddApiServiceDefaults

Configures complete API infrastructure for web applications:

:::code language="csharp" source="~/samples/extensions/ApiDefaults.cs" id="api_setup":::

**Key features:**
- **Complete API stack** - Controllers, OpenAPI, gRPC, authentication, and problem details
- **Security first** - Optional authentication with proper error handling
- **Documentation ready** - Automatic OpenAPI with XML documentation
- **Developer experience** - Rich development tools with Scalar viewer

:::code language="csharp" source="~/samples/extensions/ApiDefaults.cs" id="auth_config":::

> [!WARNING]
> Set `requireAuth: false` only for public APIs that don't need authentication.

### ConfigureApiUsingDefaults

Establishes the complete request pipeline with environment-specific optimizations:

:::code language="csharp" source="~/samples/extensions/ApiPipeline.cs" id="pipeline_config":::

**Key features:**
- **Environment optimization** - Different behaviors for development vs production
- **Security headers** - HSTS and security headers in production
- **Development tools** - OpenAPI UI, gRPC reflection, debugging tools
- **Optimized ordering** - Middleware pipeline for best performance

**Environment differences:**
- **Development** - OpenAPI UI, Scalar docs, gRPC reflection, detailed errors
- **Production** - HSTS headers, exception handlers, security optimizations

## gRPC extensions

Automatic discovery and registration of gRPC services using reflection-based patterns.

### MapGrpcServices

Automatically discovers and maps gRPC services:

:::code language="csharp" source="~/samples/extensions/GrpcServices.cs" id="grpc_discovery":::

**Key features:**
- **Zero configuration** - Automatic service discovery eliminates manual registration
- **Reflection-based** - Uses `BindServiceMethodAttribute` for service detection
- **Assembly boundaries** - Supports discovery across multiple assemblies
- **Type safety** - Compile-time verification of implementations

**Discovery options:**
- Entry assembly scanning (default)
- Explicit assembly specification
- Type marker-based discovery
- Attribute-based service detection

> [!NOTE]
> gRPC services are discovered using the `BindServiceMethodAttribute` from Protocol Buffers code generation.

## Health check extensions

Three-tier health monitoring strategy optimized for containerized microservices.

### MapDefaultHealthCheckEndpoints

Creates standardized health check endpoints:

:::code language="csharp" source="~/samples/extensions/HealthChecks.cs" id="health_endpoints":::

**Endpoint strategy:**

| Endpoint | Purpose | Access | Response |
|----------|---------|--------|----------|
| `/status` | Liveness probe | Public | Lightweight status |
| `/health/internal` | Readiness probe | Localhost only | Container readiness |
| `/health` | Detailed status | Authenticated | Full health details |

**Key features:**
- **Container optimized** - Designed for Kubernetes probes
- **Security conscious** - Sensitive data requires authentication
- **Performance aware** - Lightweight endpoints for high-frequency checks
- **Standards compliant** - Follows microservice best practices

**Response Examples:**
```json
// /status - Liveness
{
  "status": "Healthy"
}

// /health - Detailed (authenticated)
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0156",
  "entries": {
    "postgresql": {
      "status": "Healthy",
      "duration": "00:00:00.0123"
    },
    "wolverine": {
      "status": "Healthy", 
      "duration": "00:00:00.0033"
    }
  }
}
```

## Logging & Observability

Structured logging with OpenTelemetry integration provides comprehensive observability for distributed systems.

### UseInitializationLogger

Implements two-stage logging initialization for optimal startup performance:

```csharp
var app = builder.Build();
app.UseInitializationLogger();
```

**Benefits:**
- **Two-Stage Initialization**: Bootstrap logging available immediately, full logging configured after services
- **Startup Performance**: Reduces application startup time by deferring expensive logging setup
- **Configuration Integration**: Seamlessly transitions from bootstrap to configuration-driven logging
- **Error Resilience**: Ensures logging is available even if configuration fails

### AddLogging

Configures production-ready structured logging with Serilog and OpenTelemetry:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddLogging();
```

**Benefits:**
- **Structured Logging**: JSON-formatted logs with contextual enrichment
- **OpenTelemetry Integration**: Unified observability with traces, metrics, and logs
- **Configuration-Driven**: Log levels and outputs controlled via appsettings.json
- **Service Enrichment**: Automatic service name and version tagging
- **Performance Optimized**: Efficient serialization and batching

**Configuration Example:**
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "OpenTelemetry" }
    ]
  }
}
```

## OpenTelemetry

Comprehensive observability solution providing metrics, tracing, and logging with OTLP export capabilities.

### AddOpenTelemetry

Configures complete observability stack with industry-standard instrumentation:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddOpenTelemetry();
```

**Benefits:**
- **Complete Observability**: Metrics, traces, and logs in a unified system
- **Industry Standards**: OTLP export compatible with major observability platforms
- **Automatic Instrumentation**: ASP.NET Core, HTTP clients, and runtime metrics
- **Custom Metrics**: Wolverine messaging and business metrics support
- **Performance Monitoring**: Request tracing and performance analytics
- **Production Ready**: Optimized for high-throughput scenarios

**Instrumentation Included:**
- ASP.NET Core requests and responses
- HTTP client calls and dependencies
- .NET Runtime metrics (GC, ThreadPool, etc.)
- Wolverine messaging metrics
- Custom application metrics

**Export Formats:**
- OTLP (OpenTelemetry Protocol)
- Prometheus metrics
- Jaeger tracing
- Console output (development)

## Messaging & CQRS

Wolverine-based messaging system providing CQRS patterns, reliable messaging, and event sourcing capabilities.

### AddWolverine

Configures Wolverine messaging with PostgreSQL persistence and Kafka integration:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddWolverine(connectionString, configureOptions: options => {
    options.ConfigureKafka("localhost:9092");
});
```

**Benefits:**
- **CQRS Implementation**: Separate command and query handling with type safety
- **Reliable Messaging**: Durable queues with outbox pattern and transaction support
- **Event Sourcing**: Built-in support for event sourcing patterns
- **Cross-Service Communication**: Reliable messaging between microservices
- **Dead Letter Handling**: Automatic retry policies and error handling
- **Observability**: Full integration with OpenTelemetry and health checks

**Configuration Features:**
- PostgreSQL message persistence
- Kafka event streaming
- Automatic handler discovery
- Retry and dead letter policies
- Health check integration

### Type-Safe Messaging Extensions

Generic command and query invocation with compile-time type safety:

```csharp
// Command execution
await messageBus.InvokeCommandAsync(new CreateInvoiceCommand
{
    Amount = 100.00m,
    Currency = "USD"
});

// Query execution
var result = await messageBus.InvokeQueryAsync(new GetInvoiceQuery 
{ 
    InvoiceId = invoiceId 
});
```

**Benefits:**
- **Type Safety**: Compile-time verification of command and query types
- **IntelliSense Support**: Full IDE support with parameter completion
- **Error Prevention**: Eliminates runtime type casting errors
- **Clean Code**: Expressive syntax that clearly indicates intent
- **Testability**: Easy mocking and unit testing of message handlers

### Handler Discovery

Automatic discovery of command and query handlers across domain assemblies:

```csharp
services.ConfigureAppHandlers(); // Discovers handlers in domain assemblies
```

**Benefits:**
- **Zero Registration**: Handlers are automatically discovered and registered
- **Domain Boundaries**: Respects microservice domain boundaries
- **Convention-Based**: Uses consistent patterns for handler identification
- **Performance**: Single-pass registration with assembly caching
- **Maintainability**: New handlers are automatically included

## Database Integration

High-performance database integration using Dapper with stored procedure support and connection management.

### Stored Procedure Extensions

Type-safe stored procedure execution with generic result mapping:

```csharp
// Execute stored procedure (returns affected rows)
var rowsAffected = await dataSource.SpExecute("invoice_create", new
{
    amount = 100.00m,
    currency = "USD",
    cashier_id = cashierId
});

// Query with results
var invoices = await dataSource.SpQuery<Invoice>("invoice_list", new
{
    date_from = DateTime.Today,
    limit = 50
});

// Flexible stored procedure calls
var result = await dataSource.SpCall<decimal>("calculate_total", parameters =>
{
    parameters.Add("invoice_id", invoiceId);
    parameters.Add("tax_rate", 0.08m);
});
```

**Benefits:**
- **Type Safety**: Generic result mapping with compile-time verification
- **Performance**: Direct stored procedure execution without ORM overhead
- **Parameter Safety**: Automatic parameter binding with SQL injection protection
- **Flexibility**: Support for complex parameter scenarios and output parameters
- **Connection Management**: Automatic connection lifecycle management
- **Cancellation Support**: Full async/await with cancellation token support

**Use Cases:**
- High-performance data operations
- Complex business logic in stored procedures
- Bulk data processing
- Reporting and analytics queries

## OpenAPI Documentation

Automatic OpenAPI documentation generation with XML documentation integration and custom transformers.

### AddOpenApiWithXmlDocSupport

Comprehensive OpenAPI setup with XML documentation integration:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApiWithXmlDocSupport();
```

**Benefits:**
- **Rich Documentation**: Automatic integration of XML documentation comments
- **Custom Transformers**: Document, operation, and schema-level customizations
- **Type Conversion**: Comprehensive .NET type to OpenAPI schema mapping
- **Response Types**: Automatic detection and documentation of response types
- **Examples**: Rich examples extracted from XML documentation
- **Standards Compliant**: Full OpenAPI 3.0 specification support

**XML Documentation Integration:**
```csharp
/// <summary>
/// Creates a new invoice for the specified cashier.
/// </summary>
/// <param name="request">The invoice creation details</param>
/// <returns>The created invoice with assigned ID</returns>
/// <response code="201">Invoice created successfully</response>
/// <response code="400">Invalid request data</response>
[HttpPost]
public async Task<ActionResult<Invoice>> CreateInvoice([FromBody] CreateInvoiceRequest request)
```

### Type Conversion Extensions

Automatic conversion of .NET types to OpenAPI schemas:

```csharp
var openApiType = typeof(decimal?).ConvertToOpenApiType();
// Returns: { "type": "number", "format": "decimal", "nullable": true }
```

**Supported Types:**
- Primitive types (int, string, bool, decimal, etc.)
- Nullable types
- Collections and arrays
- Complex object types
- Enum types with descriptions
- Generic types

## Utility Extensions

High-performance utility extensions for common development tasks.

### String Extensions

Optimized string case conversion utilities:

```csharp
var snakeCase = "MyPropertyName".ToSnakeCase();     // "my_property_name"
var kebabCase = "MyPropertyName".ToKebabCase();     // "my-property-name"
var customCase = "MyPropertyName".ToLowerCaseWithSeparator('_'); // Custom separator
```

**Benefits:**
- **High Performance**: Span-based algorithms minimize memory allocations
- **Acronym Handling**: Proper handling of acronyms and abbreviations
- **Memory Efficient**: Zero-allocation algorithms for large-scale operations
- **Unicode Safe**: Proper handling of international characters
- **Consistent**: Standardized case conversion across the platform

**Performance Characteristics:**
- Zero memory allocations for most operations
- Optimized for high-frequency scenarios
- Benchmark-tested performance
- Culture-invariant operations

### Source Generator Extensions

Utilities for compile-time code generation and analysis:

```csharp
// Extract attribute arguments with type safety
var value = attributeData.GetConstructorArgument<string>(0);

// Get fully qualified type names
var qualifiedName = typeSymbol.GetQualifiedName();

// Generate type declarations
var declaration = namedTypeSymbol.GetTypeDeclaration();
```

**Benefits:**
- **Compile-Time Safety**: Type-safe attribute parsing and analysis
- **Code Generation**: Utilities for generating clean, readable code
- **Symbol Analysis**: Deep inspection of type hierarchies and metadata
- **Performance**: Optimized for compile-time execution
- **Maintainability**: Consistent code generation patterns

**Use Cases:**
- Custom source generators
- Code analysis tools
- Compile-time validation
- Automatic code generation

## Design Patterns and Best Practices

### Configuration-Driven Architecture

All extensions follow a consistent pattern of reading from `IConfiguration` while providing sensible defaults:

```csharp
// Default configuration
builder.AddServiceDefaults();

// Custom configuration
builder.AddServiceDefaults(configureOptions: options => {
    options.ServiceName = "CustomService";
    options.EnableDetailedMetrics = true;
});
```

### Builder Pattern Integration

Extensions integrate seamlessly with .NET's `WebApplicationBuilder` and service registration patterns:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Chained configuration
builder.AddServiceDefaults()
       .AddApiServiceDefaults()
       .AddCustomServices();
```

### Domain Assembly Discovery

Multiple extensions use `DomainAssemblyAttribute.GetDomainAssemblies()` for automatic registration across domain boundaries:

```csharp
[assembly: DomainAssembly]

// Automatic discovery in extensions
var domainAssemblies = DomainAssemblyAttribute.GetDomainAssemblies();
```

### Health Check Integration

Database and messaging extensions automatically register health checks for comprehensive monitoring:

```csharp
// Automatic health check registration
builder.AddWolverine(connectionString); // Registers Wolverine health checks
builder.Services.AddNpgsqlDataSource(connectionString); // Registers PostgreSQL health checks
```

### Environment-Aware Configuration

Many extensions adapt behavior based on the hosting environment:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
    app.UseGrpcReflection();
}
else
{
    app.UseHsts();
    app.UseExceptionHandler();
}
```

## Getting Started

To use the Platform extensions in your microservice:

1. **Add Package References:**
```xml
<PackageReference Include="Operations.ServiceDefaults" />
<PackageReference Include="Operations.ServiceDefaults.Api" />
<PackageReference Include="Operations.Extensions" />
```

2. **Configure Basic Service:**
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

var app = builder.Build();
await app.RunAsync();
```

3. **Add API Capabilities:**
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddApiServiceDefaults();

var app = builder.Build();
app.ConfigureApiUsingDefaults();
app.MapDefaultHealthCheckEndpoints();

await app.RunAsync();
```

4. **Add messaging:**
:::code language="csharp" source="~/samples/extensions/MessagingSetup.cs" id="messaging_config":::

> [!TIP]
> Use `builder.AddServiceDefaults()` as the foundation, then layer additional capabilities like APIs, messaging, or background processing.

:::moniker-end

## Additional resources

- [Platform architecture](../architecture.md) - Core design principles and patterns
- [API development](../api/overview.md) - Building REST and gRPC services  
- [Messaging patterns](../messaging/overview.md) - Event-driven architecture with Wolverine
- [Database integration](../database-integration.md) - Data access patterns and performance
- [Source generators](../source-generators/overview.md) - High-performance code generation
- [Extension samples](https://github.com/operations-platform/extension-samples) - Complete usage examples