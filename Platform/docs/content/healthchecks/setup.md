---
title: Health Check Setup
description: Advanced configuration options for setting up health checks in your application.
---

# Health Check Setup

This document provides advanced configuration options for setting up health checks in your application, building upon the basic setup provided by the Platform's Service Defaults.

## Customizing Health Check Services

You can customize the `IHealthCheckService` registration to control various aspects of health check behavior, such as the delay before the first check, and the period between checks.

[!code-csharp[](~/docs/samples/healthchecks/CustomHealthCheckServiceSetup.cs)]

## Registering Custom Health Checks

Beyond the built-in health checks, you can register your own custom health checks by implementing the `IHealthCheck` interface.

[!code-csharp[](~/docs/samples/healthchecks/CustomHealthCheckRegistration.cs)]

## Health Check Publishers

Health check publishers allow you to send health check results to external systems or logs. You can implement custom publishers or use existing ones like the Application Insights publisher.

[!code-csharp[](~/docs/samples/healthchecks/HealthCheckPublisherSetup.cs)]

## See also

*   [Health Checks Overview](overview.md)