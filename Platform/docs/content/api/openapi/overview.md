---
title: OpenAPI Documentation - Getting Started
description: This guide covers the OpenAPI documentation features in the Operations platform.
---

# OpenAPI Documentation - Getting Started

This guide covers the OpenAPI documentation features in the Operations platform.

## Overview

The Operations platform provides comprehensive OpenAPI documentation support through built-in transformers, XML documentation integration, and automatic response type generation.

## Quick Start

### Basic Setup

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add service defaults (includes OpenAPI)
builder.AddServiceDefaults();

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

// Map OpenAPI endpoints
app.MapOpenApi();

// Optional: Add Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Operations API v1");
    });
}

app.Run();
```

### XML Documentation

Enable XML documentation in your project file:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <DocumentationFile>bin\$(Configuration)\$(TargetFramework)\$(AssemblyName).xml</DocumentationFile>
  </PropertyGroup>
</Project>
```

## Features

### Automatic Response Types

Controllers automatically generate appropriate response types:

```csharp
[ApiController]
[Route("api/[controller]")]
public class CashiersController : ControllerBase
{
    /// <summary>
    /// Creates a new cashier
    /// </summary>
    /// <param name="command">The cashier creation command</param>
    /// <returns>The created cashier</returns>
    [HttpPost]
    public async Task<ActionResult<CashierResponse>> CreateCashier(CreateCashierCommand command)
    {
        // Implementation
    }
}
```

### XML Documentation Integration

The platform automatically incorporates XML documentation comments into the OpenAPI specification:

```csharp
/// <summary>
/// Retrieves a cashier by ID
/// </summary>
/// <param name="id">The unique identifier of the cashier</param>
/// <returns>The cashier details</returns>
/// <response code="200">Returns the cashier</response>
/// <response code="404">If the cashier is not found</response>
[HttpGet("{id}")]
[ProducesResponseType<CashierResponse>(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<CashierResponse>> GetCashier(Guid id)
{
    // Implementation
}
```

### Custom Transformers

The platform includes several built-in transformers:

- **XmlDocumentationDocumentTransformer**: Applies XML documentation to the OpenAPI document
- **XmlDocumentationOperationTransformer**: Applies XML documentation to operations
- **XmlDocumentationSchemaTransformer**: Applies XML documentation to schemas

## Configuration

### OpenAPI Options

```csharp
builder.Services.Configure<OpenApiOptions>(options =>
{
    options.AddDocumentTransformer<XmlDocumentationDocumentTransformer>();
    options.AddOperationTransformer<XmlDocumentationOperationTransformer>();
    options.AddSchemaTransformer<XmlDocumentationSchemaTransformer>();
});
```

### Document Information

```csharp
builder.Services.ConfigureOpenApi(options =>
{
    options.Info = new OpenApiInfo
    {
        Title = "Operations API",
        Version = "v1",
        Description = "Operations platform API documentation",
        Contact = new OpenApiContact
        {
            Name = "Operations Team",
            Email = "team@operations.com"
        }
    };
});
```

## Best Practices

1. **Comprehensive Documentation**: Include detailed XML comments for all public APIs
2. **Response Types**: Use `ProducesResponseType` attributes for all possible responses
3. **Parameter Documentation**: Document all parameters, especially complex types
4. **Examples**: Include request/response examples where helpful
5. **Error Handling**: Document all error scenarios and status codes

## Advanced Features

### Custom Response Types

```csharp
/// <summary>
/// Updates a cashier
/// </summary>
/// <param name="id">The cashier ID</param>
/// <param name="command">The update command</param>
/// <returns>The updated cashier</returns>
/// <response code="200">Cashier updated successfully</response>
/// <response code="400">Invalid request data</response>
/// <response code="404">Cashier not found</response>
[HttpPut("{id}")]
[ProducesResponseType<CashierResponse>(StatusCodes.Status200OK)]
[ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
public async Task<ActionResult<CashierResponse>> UpdateCashier(Guid id, UpdateCashierCommand command)
{
    // Implementation
}
```

### Tags and Grouping

```csharp
[ApiController]
[Route("api/[controller]")]
[Tags("Cashiers")]
public class CashiersController : ControllerBase
{
    // Controller actions
}
```

## See Also

- [XML Documentation](xml-documentation.md)
- [Transformers](transformers.md)
- [API Overview](../overview.md)
