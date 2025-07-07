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

```csharp
using Wolverine.Runtime.Middleware;
using Wolverine.Runtime;
using System.Threading.Tasks;

public class ExceptionHandlingFrame : IChainableHandler
{
    public Task Handle(MessageContext context, CancellationToken cancellationToken)
    {
        try
        {
            // Execute the next step in the pipeline
            return context.Next(cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing message: {ex.Message}");
            // Log the exception, send a dead-letter message, etc.
            return Task.CompletedTask;
        }
    }
}
```

### Fluent Validation Executor

This middleware integrates FluentValidation into your messaging pipeline, automatically validating incoming messages.

```csharp
using FluentValidation;
using Wolverine.Runtime.Middleware;
using Wolverine.Runtime;
using System.Threading.Tasks;

public class FluentValidationExecutor : IChainableHandler
{
    private readonly IValidatorFactory _validatorFactory;

    public FluentValidationExecutor(IValidatorFactory validatorFactory)
    {
        _validatorFactory = validatorFactory;
    }

    public async Task Handle(MessageContext context, CancellationToken cancellationToken)
    {
        var message = context.Envelope.Message;
        var validator = _validatorFactory.GetValidator(message.GetType());

        if (validator != null)
        {
            var validationContext = new ValidationContext<object>(message);
            var validationResult = await validator.ValidateAsync(validationContext, cancellationToken);

            if (!validationResult.IsValid)
            {
                Console.WriteLine($"Validation failed for message: {message.GetType().Name}");
                foreach (var error in validationResult.Errors)
                {
                    Console.WriteLine($" - {error.PropertyName}: {error.ErrorMessage}");
                }
                // Throw an exception or return a validation failure result
                return;
            }
        }

        await context.Next(cancellationToken);
    }
}
```

### OpenTelemetry Instrumentation Middleware

This middleware integrates OpenTelemetry tracing and metrics into your message processing, providing distributed tracing and performance insights.

```csharp
using System.Diagnostics;
using Wolverine.Runtime.Middleware;
using Wolverine.Runtime;
using System.Threading.Tasks;

public class OpenTelemetryInstrumentationMiddleware : IChainableHandler
{
    private readonly ActivitySource _activitySource;

    public OpenTelemetryInstrumentationMiddleware(ActivitySource activitySource)
    {
        _activitySource = activitySource;
    }

    public async Task Handle(MessageContext context, CancellationToken cancellationToken)
    {
        using (var activity = _activitySource.StartActivity($"ProcessMessage: {context.Envelope.MessageType.Name}"))
        {
            activity?.SetTag("message.type", context.Envelope.MessageType.Name);
            await context.Next(cancellationToken);
        }
    }
}
```

### Request Performance Middleware

This middleware measures the performance of message processing, logging the duration of each message handling operation.

```csharp
using System.Diagnostics;
using Wolverine.Runtime.Middleware;
using Wolverine.Runtime;
using System.Threading.Tasks;

public class RequestPerformanceMiddleware : IChainableHandler
{
    public async Task Handle(MessageContext context, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await context.Next(cancellationToken);
        }
        finally
        {
            stopwatch.Stop();
            Console.WriteLine($"Message {context.Envelope.MessageType.Name} processed in {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}
```

## Custom Middlewares

You can create your own custom middlewares by implementing the `IChainableHandler` interface or by using Wolverine's built-in middleware capabilities.

## See also

*   [Wolverine Messaging](../wolverine-setup.md)
*   [FluentValidation](https://fluentvalidation.net/)
*   [OpenTelemetry](https://opentelemetry.io/)
