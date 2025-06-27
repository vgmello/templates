# Platform Extensions

The Platform service provides a comprehensive set of extension methods that enable rapid development of .NET microservices with built-in observability, messaging, database integration, and API capabilities. These extensions follow .NET's configuration pattern and provide opinionated defaults while maintaining flexibility for customization.

## Core Service Configuration

The core service configuration extensions provide the foundation for all Platform-based services, establishing consistent patterns for logging, observability, messaging, and service discovery.

### AddServiceDefaults

Configures essential service infrastructure with production-ready defaults:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

var app = builder.Build();
await app.RunAsync();
```

**Benefits:**
- **Comprehensive Setup**: Automatically configures logging, OpenTelemetry, Wolverine messaging, validators, health checks, and service discovery
- **Production Ready**: Includes resilience patterns for HTTP clients and standard error handling
- **Convention Over Configuration**: Reduces boilerplate while maintaining customization options
- **Service Discovery**: Automatic registration with .NET Aspire for seamless inter-service communication

**Configuration Options:**
- Service discovery endpoints
- Resilience handler policies
- Health check configurations
- Wolverine command-line integration

### AddValidators

Automatically discovers and registers FluentValidation validators across domain boundaries:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddValidators();
```

**Benefits:**
- **Auto-Discovery**: Scans entry assembly and domain assemblies for validator types
- **Domain Boundary Awareness**: Uses `DomainAssemblyAttribute` to discover validators across microservice boundaries
- **Consistent Validation**: Ensures uniform validation patterns across all services
- **Performance Optimized**: Single-pass registration with assembly caching

### Enhanced Application Runner

The `RunAsync` extension provides enhanced application lifecycle management:

```csharp
var app = builder.Build();
await app.RunAsync(); // Enhanced runner with Wolverine integration
```

**Benefits:**
- **Wolverine Integration**: Handles command-line operations (check-env, codegen, db-apply)
- **Graceful Shutdown**: Proper resource cleanup and connection management
- **Exception Handling**: Centralized error handling with observability integration
- **Development Tools**: Built-in support for code generation and environment validation

## API & Web Configuration

API extensions provide opinionated defaults for building RESTful APIs and gRPC services with comprehensive documentation and security features.

### AddApiServiceDefaults

Configures complete API infrastructure for web applications:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddApiServiceDefaults();

var app = builder.Build();
app.ConfigureApiUsingDefaults();
```

**Benefits:**
- **Complete API Stack**: Controllers, OpenAPI, gRPC, authentication, and problem details
- **Security First**: Optional authentication requirement with proper error handling
- **Documentation Ready**: Automatic OpenAPI generation with XML documentation support
- **gRPC Integration**: Seamless REST and gRPC service hosting
- **Developer Experience**: Rich development tools with Scalar documentation viewer

**Configuration Options:**
```csharp
builder.AddApiServiceDefaults(requireAuth: true); // Enforce authentication
```

### ConfigureApiUsingDefaults

Establishes the complete request pipeline with environment-specific optimizations:

```csharp
app.ConfigureApiUsingDefaults();
```

**Benefits:**
- **Environment Optimization**: Different behaviors for development vs production
- **Security Headers**: HSTS and security headers in production
- **Development Tools**: OpenAPI UI, gRPC reflection, and debugging tools
- **Performance**: Optimized middleware pipeline ordering
- **Observability**: Integrated request tracking and error monitoring

**Features by Environment:**
- **Development**: OpenAPI UI, Scalar documentation, gRPC reflection, detailed error pages
- **Production**: HSTS headers, exception handlers, security optimizations

## gRPC Service Registration

Automatic discovery and registration of gRPC services using reflection-based patterns.

### MapGrpcServices

Automatically discovers and maps gRPC services in assemblies:

```csharp
// Auto-discover from entry assembly
app.MapGrpcServices();

// Discover from specific assembly
app.MapGrpcServices(typeof(MyService).Assembly);

// Discover using type marker
app.MapGrpcServices<MyServiceMarker>();
```

**Benefits:**
- **Zero Configuration**: Automatic service discovery eliminates manual registration
- **Reflection-Based**: Uses `BindServiceMethodAttribute` for precise service detection
- **Assembly Boundaries**: Supports discovery across multiple assemblies
- **Type Safety**: Compile-time verification of service implementations
- **Maintainability**: Services are automatically included without code changes

**Discovery Mechanisms:**
- Entry assembly scanning
- Explicit assembly specification
- Type marker-based discovery
- Attribute-based service detection

## Health Checks

The health check extensions implement a three-tier health monitoring strategy optimized for containerized microservices.

### MapDefaultHealthCheckEndpoints

Creates standardized health check endpoints with security and performance considerations:

```csharp
var app = builder.Build();
app.MapDefaultHealthCheckEndpoints();
```

**Health Check Endpoints:**

| Endpoint | Purpose | Access | Response |
|----------|---------|--------|----------|
| `/status` | Liveness probe | Public | Lightweight status |
| `/health/internal` | Readiness probe | Localhost only | Container readiness |
| `/health` | Detailed status | Authenticated | Full health details |

**Benefits:**
- **Container Optimized**: Designed for Kubernetes liveness and readiness probes
- **Security Conscious**: Sensitive health data requires authentication
- **Performance Aware**: Lightweight endpoints for high-frequency checks
- **Observability Integration**: Health status tracking and metrics collection
- **Standards Compliant**: Follows microservice health check best practices

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

4. **Add Messaging:**
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddWolverine(connectionString);

var app = builder.Build();
await app.RunAsync();
```

This extension ecosystem provides a complete foundation for microservices development with observability, messaging, database integration, and API development capabilities built-in, following .NET best practices and industry standards.