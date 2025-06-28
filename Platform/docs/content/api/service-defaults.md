---
title: API Service Defaults
description: Learn how to configure your API services and pipeline with sensible defaults using the Operations.ServiceDefaults.Api extensions.
---

# API service defaults

The `Operations.ServiceDefaults.Api` project provides extension methods to quickly configure your ASP.NET Core application with a standard set of services and middleware for building robust APIs.

## Standard API configuration

You can easily add a conventional set of services to your application using the `AddApiServiceDefaults` method. This method sets up everything you need for a modern API, including controllers, OpenAPI support, gRPC, and authentication.

`AddApiServiceDefaults` registers the following services:
-   **Controllers**: For building MVC-style APIs.
-   **Endpoints API Explorer**: To enable OpenAPI generation.
-   **Problem Details**: For standardized error responses.
-   **OpenAPI with XML Docs**: For rich Swagger/OpenAPI documentation.
-   **HTTP Logging**: To log HTTP requests and responses.
-   **gRPC**: For high-performance RPC services.
-   **Authentication & Authorization**: To secure your endpoints.

It also configures Kestrel to disable the `Server` header for improved security.

## Default API pipeline

After building your `WebApplication`, you can configure the HTTP request pipeline with the `ConfigureApiUsingDefaults` method. This sets up a standard middleware pipeline that's ready for both development and production environments.

In all environments, the pipeline includes:
-   HTTP Logging
-   Routing
-   Authentication & Authorization
-   gRPC-Web

In development, it adds:
-   OpenAPI endpoint mapping
-   Scalar API reference UI
-   gRPC Reflection service

In production, it enables:
-   HSTS (HTTP Strict Transport Security)
-   Exception Handling

By default, all mapped controllers will require authorization. You can change this by passing `requireAuth: false`.

## Usage example

To apply the default configurations, call `AddApiServiceDefaults` on your `WebApplicationBuilder` and `ConfigureApiUsingDefaults` on the `WebApplication`.

[!code-csharp[](~/samples/api-defaults/Program.cs?highlight=9,14)]

## See also

-   [OpenAPI & XML Documentation](./openapi/overview.md)
-   [gRPC Services](./grpc.md)