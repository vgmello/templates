---
title: Service Defaults Extensions
description: Discover the core extension methods that provide sensible defaults for your services in the Operations platform.
---

# Service Defaults Extensions

The `Operations.ServiceDefaults` project offers a set of foundational extension methods designed to streamline the setup of common services and configurations for your applications. These defaults aim to provide a robust and consistent starting point, reducing boilerplate and promoting best practices.

## Core Extensions

### AddServiceDefaults

The `AddServiceDefaults` extension method is the primary entry point for configuring a standard set of services within your `WebApplicationBuilder`. It encapsulates common configurations for logging, telemetry, messaging, and health checks.

This method performs the following key actions:

-   **Kestrel HTTPS Configuration**: Sets up Kestrel to use HTTPS.
-   **Logging**: Integrates with Serilog for comprehensive logging.
-   **OpenTelemetry**: Configures OpenTelemetry for distributed tracing and metrics.
-   **Wolverine**: Adds Wolverine for robust messaging capabilities.
-   **FluentValidation**: Registers validators from the entry assembly and any marked domain assemblies.
-   **Health Checks**: Adds basic health check services.
-   **Service Discovery**: Integrates with service discovery mechanisms.
-   **HttpClient Defaults**: Configures `HttpClient` with standard resilience handlers.

#### Usage example

To apply these defaults, simply call `AddServiceDefaults` on your `WebApplicationBuilder`:

[!code-csharp[](~/samples/basic-service/Program.cs?highlight=7)]

### AddValidators

The `AddValidators` extension method specifically registers FluentValidation validators. It automatically discovers validators from the entry assembly and any assemblies marked with the `DomainAssemblyAttribute`.

#### Usage example

While `AddServiceDefaults` includes this, you can call it explicitly if needed:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddValidators();
```

### RunAsync

The `RunAsync` extension method for `WebApplication` provides a standardized way to run your application, incorporating logging for initialization and handling Wolverine-specific commands.

#### Usage example

Replace the standard `app.Run()` with `app.RunAsync(args)`:

[!code-csharp[](~/samples/extensions/RunAsyncExample.cs?highlight=4)]

## See also

- [Domain Assembly Attribute](../domain-assembly-attribute.md)
- [Logging](../logging/overview.md)
- [OpenTelemetry](../opentelemetry/overview.md)
- [Wolverine Setup](../messaging/wolverine-setup.md)