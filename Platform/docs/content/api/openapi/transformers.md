# OpenAPI Transformers

This guide covers the OpenAPI transformers available in the Operations platform and how to create custom transformers.

## Overview

OpenAPI transformers in the Operations platform allow you to modify the generated OpenAPI specification at different levels - document, operation, and schema. The platform includes several built-in transformers for common scenarios.

## Built-in Transformers

### XML Documentation Transformers

#### XmlDocumentationDocumentTransformer

Applies XML documentation to the entire OpenAPI document:

```csharp
public class XmlDocumentationDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        // Applies assembly-level XML documentation
        // Sets document title, description, version from assembly attributes
        return Task.CompletedTask;
    }
}
```

#### XmlDocumentationOperationTransformer

Applies XML documentation to individual operations:

```csharp
public class XmlDocumentationOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        // Applies method-level XML documentation
        // Sets operation summary, description, parameter descriptions
        return Task.CompletedTask;
    }
}
```

#### XmlDocumentationSchemaTransformer

Applies XML documentation to schemas:

```csharp
public class XmlDocumentationSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        // Applies type-level XML documentation
        // Sets schema descriptions, property descriptions, examples
        return Task.CompletedTask;
    }
}
```

### Response Type Convention

#### AutoProducesResponseTypeConvention

Automatically adds `ProducesResponseType` attributes based on controller action return types:

```csharp
public class AutoProducesResponseTypeConvention : IApiConventionMetadataProvider
{
    public void BuildApiConventionMetadata(ApiConventionMetadataProviderContext context)
    {
        // Automatically infers response types from action methods
        // Adds appropriate ProducesResponseType attributes
    }
}
```

## Registering Transformers

### Service Registration

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register built-in transformers
builder.Services.ConfigureOpenApi(options =>
{
    options.AddDocumentTransformer<XmlDocumentationDocumentTransformer>();
    options.AddOperationTransformer<XmlDocumentationOperationTransformer>();
    options.AddSchemaTransformer<XmlDocumentationSchemaTransformer>();
});

// Register with dependency injection
builder.Services.AddSingleton<IXmlDocumentationService, XmlDocumentationService>();
```

### Automatic Registration

The service defaults automatically register common transformers:

```csharp
// Automatically registered when using AddServiceDefaults()
builder.AddServiceDefaults();
```

## Creating Custom Transformers

### Document Transformer

Create a custom document transformer to modify the entire OpenAPI specification:

```csharp
public class CustomDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document, 
        OpenApiDocumentTransformerContext context, 
        CancellationToken cancellationToken)
    {
        // Add custom security schemes
        document.Components.SecuritySchemes["ApiKey"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Name = "X-API-Key",
            Description = "API Key for authentication"
        };

        // Add global security requirement
        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "ApiKey"
                    }
                },
                new string[] { }
            }
        });

        // Add custom server information
        document.Servers = new List<OpenApiServer>
        {
            new() { Url = "https://api.operations.com", Description = "Production" },
            new() { Url = "https://staging-api.operations.com", Description = "Staging" },
            new() { Url = "https://localhost:8080", Description = "Development" }
        };

        return Task.CompletedTask;
    }
}
```

### Operation Transformer

Create a custom operation transformer to modify individual API operations:

```csharp
public class CustomOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation, 
        OpenApiOperationTransformerContext context, 
        CancellationToken cancellationToken)
    {
        // Add custom headers to all operations
        operation.Parameters ??= new List<OpenApiParameter>();
        
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Correlation-ID",
            In = ParameterLocation.Header,
            Description = "Correlation ID for request tracking",
            Required = false,
            Schema = new OpenApiSchema { Type = "string" }
        });

        // Add common error responses
        operation.Responses.TryAdd("500", new OpenApiResponse
        {
            Description = "Internal server error",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new()
                {
                    Schema = new OpenApiSchema
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.Schema,
                            Id = "ProblemDetails"
                        }
                    }
                }
            }
        });

        // Add rate limiting information
        if (context.Description.ActionDescriptor is ControllerActionDescriptor actionDescriptor)
        {
            var rateLimitAttribute = actionDescriptor.MethodInfo
                .GetCustomAttribute<RateLimitAttribute>();
            
            if (rateLimitAttribute != null)
            {
                operation.Extensions["x-rate-limit"] = new OpenApiObject
                {
                    ["requests"] = new OpenApiInteger(rateLimitAttribute.Requests),
                    ["window"] = new OpenApiString(rateLimitAttribute.Window)
                };
            }
        }

        return Task.CompletedTask;
    }
}
```

### Schema Transformer

Create a custom schema transformer to modify type schemas:

```csharp
public class CustomSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema, 
        OpenApiSchemaTransformerContext context, 
        CancellationToken cancellationToken)
    {
        // Add custom validation patterns
        if (context.JsonTypeInfo.Type == typeof(string))
        {
            var propertyName = context.JsonPropertyInfo?.Name;
            
            switch (propertyName?.ToLowerInvariant())
            {
                case "email":
                    schema.Format = "email";
                    schema.Pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                    break;
                    
                case "phone":
                    schema.Format = "phone";
                    schema.Pattern = @"^\+?[1-9]\d{1,14}$";
                    break;
                    
                case "currency":
                    schema.Pattern = @"^[A-Z]{3}$";
                    schema.Example = new OpenApiString("USD");
                    break;
            }
        }

        // Add enum descriptions
        if (schema.Enum != null && schema.Enum.Any())
        {
            var enumType = context.JsonTypeInfo.Type;
            if (enumType.IsEnum)
            {
                var enumValues = new List<string>();
                foreach (var enumValue in Enum.GetValues(enumType))
                {
                    var enumName = Enum.GetName(enumType, enumValue);
                    var description = enumType.GetField(enumName!)
                        ?.GetCustomAttribute<DescriptionAttribute>()
                        ?.Description ?? enumName;
                    
                    enumValues.Add($"{enumName}: {description}");
                }
                
                schema.Description += $"\n\nPossible values:\n- {string.Join("\n- ", enumValues)}";
            }
        }

        return Task.CompletedTask;
    }
}
```

## Advanced Transformer Scenarios

### Conditional Transformers

Apply transformers based on conditions:

```csharp
public class ConditionalTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation, 
        OpenApiOperationTransformerContext context, 
        CancellationToken cancellationToken)
    {
        // Only apply to specific controllers
        if (context.Description.ActionDescriptor is ControllerActionDescriptor actionDescriptor &&
            actionDescriptor.ControllerName.EndsWith("PublicController"))
        {
            // Remove authentication requirements for public endpoints
            operation.Security?.Clear();
        }

        // Only apply to specific HTTP methods
        if (context.Description.HttpMethod == HttpMethods.Post)
        {
            // Add specific validation for POST operations
            operation.RequestBody?.Content?.Values?.ToList().ForEach(content =>
            {
                content.Extensions["x-validation-required"] = new OpenApiBoolean(true);
            });
        }

        return Task.CompletedTask;
    }
}
```

### Multi-Version Support

Handle API versioning in transformers:

```csharp
public class VersioningTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document, 
        OpenApiDocumentTransformerContext context, 
        CancellationToken cancellationToken)
    {
        // Extract version from context
        var version = context.DocumentName ?? "v1";
        
        document.Info.Version = version;
        document.Info.Title = $"Operations API {version}";

        // Version-specific modifications
        switch (version)
        {
            case "v1":
                // Legacy support configurations
                break;
            case "v2":
                // New features and breaking changes
                break;
        }

        return Task.CompletedTask;
    }
}
```

## Registration and Configuration

### Complete Configuration Example

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureOpenApi(options =>
{
    // Document transformers (applied to entire document)
    options.AddDocumentTransformer<XmlDocumentationDocumentTransformer>();
    options.AddDocumentTransformer<CustomDocumentTransformer>();
    
    // Operation transformers (applied to each operation)
    options.AddOperationTransformer<XmlDocumentationOperationTransformer>();
    options.AddOperationTransformer<CustomOperationTransformer>();
    
    // Schema transformers (applied to each schema)
    options.AddSchemaTransformer<XmlDocumentationSchemaTransformer>();
    options.AddSchemaTransformer<CustomSchemaTransformer>();
});

// Register transformer dependencies
builder.Services.AddSingleton<IXmlDocumentationService, XmlDocumentationService>();

var app = builder.Build();

// Apply transformers to OpenAPI document
app.MapOpenApi();
```

## Best Practices

1. **Single Responsibility**: Keep transformers focused on specific concerns
2. **Performance**: Avoid expensive operations in transformers
3. **Null Checks**: Always check for null values before modifying properties
4. **Idempotent**: Ensure transformers can be applied multiple times safely
5. **Documentation**: Document custom transformers and their effects
6. **Testing**: Write unit tests for custom transformers

## Troubleshooting

### Common Issues

1. **Transformer Not Applied**: Check registration order and dependencies
2. **Null Reference Exceptions**: Add null checks in transformer logic
3. **Performance Issues**: Profile transformer execution time
4. **Conflicts**: Ensure transformers don't override each other's changes

### Debugging

Enable OpenAPI debugging:

```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.AspNetCore.OpenApi": "Debug"
    }
  }
}
```

## See Also

- [OpenAPI Overview](overview.md)
- [XML Documentation](xml-documentation.md)
- [API Documentation](../overview.md)