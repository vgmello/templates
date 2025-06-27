---
title: Dynamic Log Level Settings
description: Configure dynamic log levels for your application, allowing runtime adjustments to logging verbosity.
---

# Dynamic Log Level Settings

The `DynamicLogLevelSettings` class provides a mechanism to configure log levels dynamically within your application. This feature allows you to adjust the verbosity of logging at runtime without requiring a full application restart, which is invaluable for troubleshooting and monitoring in production environments.

## Configuration

### Properties

The `Properties` dictionary within `DynamicLogLevelSettings` allows you to define specific log levels for different loggers or categories. The keys of the dictionary represent the logger names (e.g., "Microsoft", "System", or your custom logger names), and the values are a set of log levels (e.g., "Information", "Debug", "Warning") that should be enabled for that logger.

## Usage example

You typically configure `DynamicLogLevelSettings` through your `appsettings.json` or other configuration sources. The `SectionName` constant, `"DynamicLogLevel"`, indicates the configuration section where these settings should reside.

### appsettings.json configuration

```json
{
  "DynamicLogLevel": {
    "Properties": {
      "Microsoft": [
        "Information"
      ],
      "YourApp.Controllers": [
        "Debug",
        "Information"
      ]
    }
  }
}
```

In this example:

-   Logs from the "Microsoft" category will be set to `Information` level.
-   Logs from "YourApp.Controllers" will be set to `Debug` and `Information` levels.

## See also

- [Logging Setup Extensions](./logging-setup.md)
