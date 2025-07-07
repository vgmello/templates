---
title: Logging Setup Extensions
description: Learn how to configure logging in your services using Serilog and OpenTelemetry with the provided setup extensions.
---

# Logging Setup Extensions

The `LoggingSetupExtensions` class provides convenient extension methods to configure logging in your ASP.NET Core applications. It leverages Serilog for flexible logging and integrates with OpenTelemetry for distributed tracing and metrics, ensuring comprehensive observability.

## Key Features

### UseInitializationLogger

This method sets up a bootstrap logger for use during the two-stage initialization of your application host. This is crucial for capturing any log events or exceptions that occur early in the application startup process, before the full logging infrastructure is in place.

#### Usage example

Call `UseInitializationLogger` on your `WebApplication` instance as early as possible in your `Program.cs`:

```csharp
// In Program.cs
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseInitializationLogger(); // Call this before app.Run()

// ... rest of your application setup

app.Run();
```

### AddLogging

This extension method configures Serilog as the primary logging provider for your application. It clears any existing logging providers and sets up Serilog to read its configuration from your application's `IConfiguration` and `IServiceProvider`.

It also enriches log contexts and directs logs to OpenTelemetry, making them available for tracing and metrics systems.

#### Usage example

Call `AddLogging` on your `IHostApplicationBuilder` (which `WebApplicationBuilder` implements):

[!code-csharp[](~/samples/logging/Program.cs?highlight=3)]

### ConfigureLogger

This static method provides the core logic for configuring a `LoggerConfiguration` for Serilog. It reads configuration from both `IConfiguration` and `IServiceProvider`, enriches logs with context, and outputs them to OpenTelemetry.

You typically won't call this method directly, as it's used internally by `AddLogging`.

## Configuration

Serilog's configuration can be managed through your `appsettings.json` file. You can define sinks (where logs are sent), minimum log levels, and enrichers.

### Example appsettings.json for logging

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
      { "Name": "Console" }
    ],
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithProcessId",
      "WithThreadId"
    ]
  }
}
```

## See also

- [Logging Overview](./overview.md)
- [Dynamic Log Levels](./dynamic-log-levels.md)
- [OpenTelemetry Overview](../opentelemetry/overview.md)
