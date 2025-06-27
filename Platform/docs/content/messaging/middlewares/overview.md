---
title: Messaging Middlewares
description: Understand how to use and create custom messaging middlewares to intercept and process messages in your application.
---

# Messaging Middlewares

Messaging middlewares provide a powerful way to intercept and process messages as they flow through your application's messaging pipeline. They allow you to implement cross-cutting concerns such as logging, error handling, validation, and telemetry in a modular and reusable manner.

## Understanding Middlewares

Middlewares are typically organized in a pipeline, where each middleware performs a specific task and then passes the message to the next middleware in the chain. This pattern is similar to ASP.NET Core's HTTP request pipeline.

## Key Middlewares

### Exception Handling Frame

The exception handling frame middleware catches exceptions that occur during message processing, allowing you to implement centralized error handling logic.

[!code-csharp[](~/docs/samples/messaging/middlewares/ExceptionHandlingFrame.cs)]

### Fluent Validation Executor

This middleware integrates FluentValidation into your messaging pipeline, automatically validating incoming messages.

[!code-csharp[](~/docs/samples/messaging/middlewares/FluentValidationExecutor.cs)]

### OpenTelemetry Instrumentation Middleware

This middleware integrates OpenTelemetry tracing and metrics into your message processing, providing distributed tracing and performance insights.

[!code-csharp[](~/docs/samples/messaging/middlewares/OpenTelemetryInstrumentationMiddleware.cs)]

### Request Performance Middleware

This middleware measures the performance of message processing, logging the duration of each message handling operation.

[!code-csharp[](~/docs/samples/messaging/middlewares/RequestPerformanceMiddleware.cs)]

## Custom Middlewares

You can create your own custom middlewares by implementing the `IChainableHandler` interface or by using Wolverine's built-in middleware capabilities.

## See also

*   [Wolverine Messaging](wolverine-integration.md)
*   [FluentValidation](https://fluentvalidation.net/)
*   [OpenTelemetry](https://opentelemetry.io/)
