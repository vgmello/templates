---
title: OpenAPI & XML Documentation
description: Learn how to enhance your OpenAPI documentation with automatic response types and rich XML comments.
---

# OpenAPI & XML documentation

The `Operations.ServiceDefaults.Api` project provides powerful features to improve your OpenAPI documentation, making it more accurate and descriptive with minimal effort.

## Automatic success response types

When you build APIs, it's crucial to document the expected responses for each endpoint. The `AutoProducesResponseTypeConvention` automatically adds a `[ProducesResponseType]` attribute to your controller actions that return an `ActionResult<T>`.

This convention inspects your actions and, if no existing success (2xx) response attribute is found, it infers the response type from the `T` in `ActionResult<T>`. This saves you from manually decorating every action, ensuring your OpenAPI specification is always in sync with your code.

It uses a special, non-standard status code (`-299`) as a marker. This marker is later processed by the `XmlDocumentationOperationTransformer` to replace it with the correct 200 (OK) status code and description from your XML comments.

### How it works

1.  The convention scans all controller actions.
2.  It checks if an action returns `Task<ActionResult<T>>`, `ValueTask<ActionResult<T>>`, or `ActionResult<T>`.
3.  If no `[ProducesResponseType]` for a 2xx status code is already present, it adds one using the type `T` and the special `-299` status code.

This process is entirely automatic when you use `AddOpenApiWithXmlDocSupport()`.

## Rich documentation from XML comments

Beyond automatic response types, the system integrates deeply with your XML documentation comments to enrich the OpenAPI output. By enabling XML documentation in your project, you can add detailed descriptions, summaries, and examples to your actions, parameters, and schemas.

### Setup

To enable this feature, you need to:

1.  **Enable XML documentation** in your `.csproj` file:

    ```xml
    <PropertyGroup>
      <GenerateDocumentationFile>true</GenerateDocumentationFile>
      <NoWarn>$(NoWarn);1591</NoWarn>
    </PropertyGroup>
    ```

2.  **Register the services** in your `Program.cs`:

    ```csharp
    builder.Services.AddOpenApiWithXmlDocSupport();
    ```

### How it works

The `AddOpenApiWithXmlDocSupport` extension method registers several key components:

-   **`XmlDocumentationService`**: A singleton service that parses and caches all the XML documentation files from your application's assemblies.
-   **Transformers**: A set of `IDocument`, `IOperation`, and `ISchema` transformers that read the cached XML comments and apply them to the corresponding OpenAPI elements.
    -   `XmlDocumentationDocumentTransformer`: Adds descriptions to tags (controllers).
    -   `XmlDocumentationOperationTransformer`: Adds summaries and remarks to operations (actions) and replaces the `-299` status code with a proper 200 OK response from your `<response>` tags.
    -   `XmlDocumentationSchemaTransformer`: Adds descriptions and examples to schemas and their properties.

## Usage example

By combining XML comments with the automated conventions, you can produce a rich and accurate OpenAPI specification.

Consider the following controller action:

```csharp
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SampleApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        /// <summary>
        /// Get the current weather forecast.
        /// </summary>
        /// <remarks>
        /// This endpoint returns a 5-day weather forecast.
        /// </remarks>
        /// <response code="200">Returns the weather forecast.</response>
        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<ActionResult<WeatherForecast>> Get()
        {
            return Ok(new WeatherForecast());
        }
    }
}
```

And the corresponding `WeatherForecast` model:

```csharp
using System;

namespace SampleApp
{
    public class WeatherForecast
    {
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public string? Summary { get; set; }
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}
```

When the OpenAPI documentation is generated, it will automatically include:
-   The summary and remarks for the `GET` endpoint.
-   A 200 OK response with the description from the `<response>` tag.
-   The description for the `WeatherForecast` schema and its properties.

## XML documentation service

The `XmlDocumentationService` is the core component responsible for parsing and providing access to your XML documentation comments. It's registered as a singleton service and exposes the `IXmlDocumentationService` interface.

### Loading documentation

The service can load an XML documentation file asynchronously with the `LoadDocumentationAsync` method. It reads the file, parses the XML, and stores the documentation for each member (types, methods, properties) in an in-memory cache.

### Retrieving documentation

Once loaded, you can retrieve the documentation for a specific code element using methods like:
-   `GetMethodDocumentation(MethodInfo)`
-   `GetTypeDocumentation(Type)`
-   `GetPropertyDocumentation(PropertyInfo)`

These methods return an `XmlDocumentationInfo` object, which contains the parsed summary, remarks, parameters, responses, and example text.

### Caching

The parsed documentation is cached in a `ConcurrentDictionary` for fast retrieval. The cache is cleared when `ClearCache()` is called, which typically happens after a request in the `OpenApiCachingMiddleware` to ensure documentation is up-to-date during development.

## Enriching OpenAPI schema

The `OpenApiDocExtensions` class provides extension methods to enrich your OpenAPI schema with information from the XML documentation.

### Converting types

The `ConvertToOpenApiType` method converts a string value to its corresponding `IOpenApiPrimitive` type. This is used to set the `Example` field in the OpenAPI schema.

### Enriching with XML documentation

The `EnrichWithXmlDocInfo` method takes an `OpenApiSchema` and an `XmlDocumentationInfo` object and enriches the schema with the summary, remarks, and example from the XML documentation.

## Adding OpenAPI with XML documentation support

The `AddOpenApiWithXmlDocSupport` extension method is the easiest way to add OpenAPI to your application with XML documentation support. It registers the `XmlDocumentationService`, the OpenAPI transformers, and the `AutoProducesResponseTypeConvention`.

## Document transformers

The `XmlDocumentationDocumentTransformer` enriches the OpenAPI document with information from the assembly and XML documentation.

### Enriching tags

The transformer enriches the tags in the OpenAPI document with the summary and remarks from the XML documentation of the corresponding controller.

### Enriching document info

The transformer enriches the document info with the company and copyright information from the assembly.

### Adding metadata

The transformer adds the assembly version to the OpenAPI document.

## Operation transformers

The `XmlDocumentationOperationTransformer` enriches the operations in the OpenAPI document with information from the XML documentation.

### Enriching operations

The transformer enriches the operations with the summary, remarks, and operation ID from the XML documentation.

### Enriching parameters

The transformer enriches the parameters with the description and example from the XML documentation.

### Enriching responses

The transformer enriches the responses with the description from the XML documentation.

## Schema transformers

The `XmlDocumentationSchemaTransformer` is responsible for enriching the OpenAPI schemas of your data models with information from your XML documentation comments.

### Enriching schemas

For each type, the transformer fetches the corresponding XML documentation and applies the summary, remarks, and example to the schema's description and example fields.

### Enriching properties

The transformer iterates through each property of a schema and applies the XML documentation from the corresponding class property. It correctly handles property names that are customized with the `[JsonPropertyName]` attribute.

Additionally, it adds type-specific information:
-   **Nullable types**: It correctly marks properties of nullable types (e.g., `string?`, `int?`) as `nullable: true` in the OpenAPI schema.
-   **Enums**: It populates the `enum` field of the schema with the names of the enum members.

## Caching for performance

To improve the performance of your development environment, the `OpenApiCachingMiddleware` is included when you use `ConfigureApiUsingDefaults`. This middleware intercepts requests to your OpenAPI endpoint (e.g., `/openapi/v1.json`) and caches the generated JSON or YAML document on disk.

### How it works

1.  **Request Interception**: The middleware checks if an incoming request is for an OpenAPI document.
2.  **Cache Check**: It generates a cache key based on the request path and checks if a corresponding cached file exists.
3.  **Serve from Cache**: If a valid cache file is found, it serves the document directly from the disk, using `ETag` and `Last-Modified` headers for efficient browser caching. This avoids re-generating the document on every request.
4.  **Generate and Cache**: If no cache file exists, it executes the request pipeline to generate the OpenAPI document, saves the response to a cache file in a temporary directory, and then serves it to the client.

This caching mechanism significantly speeds up the load times of tools like Scalar and Swagger UI during development.

> [!NOTE]
> The `OpenApiCachingMiddleware` is only active in the development environment.

## See also

-   [API Service Defaults](../service-defaults.md)