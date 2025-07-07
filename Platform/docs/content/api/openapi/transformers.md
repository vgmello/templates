---
title: OpenAPI Transformers
description: Learn about the various OpenAPI transformers used in the Platform to enrich and customize your API documentation.
---

# OpenAPI Transformers

OpenAPI transformers are powerful components that allow you to modify and enrich the generated OpenAPI (Swagger) document. The Platform utilizes several built-in transformers to automatically incorporate XML documentation, handle response types, and ensure your API documentation is accurate and comprehensive.

## Understanding Transformers

Transformers operate on different parts of the OpenAPI document:

*   **Document Transformers**: Modify the overall OpenAPI document, such as adding global tags or security definitions.
*   **Operation Transformers**: Modify individual API operations (endpoints), such as adding summaries, descriptions, parameters, or response types.
*   **Schema Transformers**: Modify the schemas (data models) used in your API, such as adding property descriptions, examples, or handling enum values.

## Built-in Transformers

The Platform provides the following key transformers:

### XmlDocumentationDocumentTransformer

This transformer enriches the OpenAPI document with information extracted from your assembly's XML documentation. It can add descriptions to tags (which often correspond to controllers) and include assembly-level metadata like company and copyright information.

### XmlDocumentationOperationTransformer

This transformer is responsible for applying XML documentation comments to individual API operations. It extracts summaries, remarks, and parameter descriptions from your method's XML comments and populates the corresponding fields in the OpenAPI specification. Crucially, it also handles the automatic conversion of the special `-299` status code (used by `AutoProducesResponseTypeConvention`) to a proper 200 OK response based on your `<response>` XML tags.

### XmlDocumentationSchemaTransformer

This transformer enriches the OpenAPI schemas (your data models) with details from their XML documentation. It adds descriptions and examples to the schema itself and to individual properties within the schema. It also correctly handles nullable types and populates enum values.

## How to Use

These transformers are automatically registered when you use the `AddOpenApiWithXmlDocSupport()` extension method in your `Program.cs`.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Operations.ServiceDefaults.Api.OpenApi.Extensions;

public class OpenApiTransformerSetup
{
    public static void ConfigureOpenApiTransformers()
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add OpenAPI with XML documentation support, which registers the transformers
        builder.Services.AddOpenApiWithXmlDocSupport();

        var app = builder.Build();
        app.MapOpenApi();
        app.Run();
    }
}
```

## Custom Transformers

You can also create your own custom transformers by implementing the `IDocumentTransformer`, `IOperationTransformer`, or `ISchemaTransformer` interfaces and registering them with your OpenAPI options.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using System.Threading.Tasks;

public class CustomOperationTransformer : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        // Add a custom header to all operations
        context.OperationDescription.Operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Custom-Header",
            Kind = OpenApiParameterKind.Header,
            Description = "A custom header for demonstration purposes.",
            IsRequired = false,
            Type = OpenApiDataType.String
        });
        return true;
    }
}

public class CustomTransformerSetup
{
    public static void ConfigureCustomTransformers()
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApiDocument(options =>
        {
            options.OperationProcessors.Add(new CustomOperationTransformer());
        });

        var app = builder.Build();
        app.MapOpenApi();
        app.Run();
    }
}
```

## See also

*   [OpenAPI Documentation Overview](overview.md)
*   [XML Documentation](xml-documentation.md)
*   [NSwag Documentation](https://github.com/RicoSuter/NSwag/wiki)
