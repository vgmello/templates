---
title: Service Bus Options
description: Configure service bus settings, including service naming and connection strings, for your messaging infrastructure.
---

# Service Bus Options

The `ServiceBusOptions` class provides a structured way to configure settings related to your application's service bus integration. This includes defining the public service name and handling connection strings for features like transactional inbox/outbox and message persistence.

## Configuration

### PublicServiceName

The `PublicServiceName` property defines the name of your service as it will be known within the messaging infrastructure. If not explicitly set, it defaults to a normalized version of your application's name.

### ServiceUrn

The `ServiceUrn` property generates a Uniform Resource Name (URN) for your service, based on the `PublicServiceName`. This URN uniquely identifies your service within the messaging system.

### Connection String

The `ServiceBusOptions` also checks for a connection string named `ServiceBus` in your application's configuration. This connection string is crucial for enabling advanced messaging features such as transactional inbox/outbox and message persistence. If the connection string is not found, these features will be disabled, and a warning will be logged.

## Usage example

You typically configure `ServiceBusOptions` through your application's `appsettings.json` or other configuration sources. The `Configurator` class, which implements `IPostConfigureOptions<ServiceBusOptions>`, automatically populates default values and validates the presence of the service bus connection string.

### appsettings.json configuration

```json
{
  "ConnectionStrings": {
    "ServiceBus": "Host=localhost;Port=5672;Username=guest;Password=guest"
  },
  "ServiceBus": {
    "PublicServiceName": "my-awesome-service"
  }
}
```

### Program.cs integration

While `ServiceBusOptions` is primarily configured via `appsettings.json`, its `Configurator` is automatically registered when you use the service defaults, ensuring proper setup and validation.

```csharp
// In Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add service defaults, which includes ServiceBusOptions configuration
builder.AddServiceDefaults();

// You can access the options like this if needed:
builder.Services.AddOptions<ServiceBusOptions>()
    .PostConfigureAll(options =>
    {
        Console.WriteLine($"Service Bus Public Name: {options.PublicServiceName}");
        Console.WriteLine($"Service Bus URN: {options.ServiceUrn}");
    });

var app = builder.Build();
app.Run();
```

## See also

- [Messaging Overview](./overview.md)
- [Wolverine Integration](./wolverine-setup.md)
