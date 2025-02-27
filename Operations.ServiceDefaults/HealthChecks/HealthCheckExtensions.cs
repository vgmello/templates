// Copyright (c) ABCDEG. All rights reserved.

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Operations.ServiceDefaults.Api.EndpointFilters;

namespace Operations.ServiceDefaults.HealthChecks;

public static class HealthCheckExtensions
{
    private const string HealthCheckLogName = "HealthChecks";

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static WebApplication MapDefaultHealthCheckEndpoints(this WebApplication app)
    {
        // If the HealthCheckStatusStore is registered in the app DI container, we will use it,
        // otherwise we will create a new one. This simplifies app registration in the majority of cases,
        // while allowing the app to query for health status if it ever needs to.
        var healthCheckStore = app.Services.GetService<HealthCheckStatusStore>() ?? new HealthCheckStatusStore();

        var logger = GetHealthCheckLogger(app);

        app.MapGet("/status", () => healthCheckStore.GetLastStatus().ToString());

        var isDevelopment = app.Environment.IsDevelopment();
        app.MapHealthChecks("/health/internal",
                new HealthCheckOptions
                {
                    ResponseWriter = (ctx, report) =>
                        ProcessHealthCheckResult(ctx, logger, healthCheckStore, report, outputResult: isDevelopment)
                })
            .RequireHost("localhost")
            .AddEndpointFilter(new LocalhostEndpointFilter(logger));

        var publicEndpoint = app.MapHealthChecks("/health",
            new HealthCheckOptions
            {
                ResponseWriter = (ctx, report) => ProcessHealthCheckResult(ctx, logger, healthCheckStore, report, outputResult: true)
            });

        publicEndpoint.RequireAuthorization();

        return app;
    }

    private static Task ProcessHealthCheckResult(
        HttpContext httpContext,
        ILogger logger,
        HealthCheckStatusStore healthCheckStore,
        HealthReport report,
        bool outputResult)
    {
        LogHealthCheckResponse(logger, report);
        healthCheckStore.StoreHealthStatus(report);

        return outputResult
            ? WriteReportObject(httpContext, report)
            : httpContext.Response.WriteAsync(report.Status.ToString());
    }

    private static void LogHealthCheckResponse(ILogger logger, HealthReport report)
    {
        if (report.Status is HealthStatus.Healthy)
        {
            logger.LogDebug("Health check response: {@HealthReport}", report);

            return;
        }

        var logLevel = report.Status == HealthStatus.Unhealthy ? LogLevel.Error : LogLevel.Warning;

        var failedHealthReport = report.Entries.Select(e =>
            new { e.Key, e.Value.Status, e.Value.Duration, Error = e.Value.Exception?.Message });

        logger.Log(logLevel, "Health check failed: {FailedHealthReport}", failedHealthReport);
    }

    private static Task WriteReportObject(HttpContext context, HealthReport report)
    {
        var response = new
        {
            Status = report.Status.ToString(),
            Duration = report.TotalDuration,
            Info = report.Entries
                .Select(e =>
                    new
                    {
                        e.Key,
                        e.Value.Description,
                        e.Value.Duration,
                        Status = Enum.GetName(typeof(HealthStatus), e.Value.Status),
                        Error = e.Value.Exception?.Message,
                        e.Value.Data
                    })
                .ToList()
        };

        return context.Response.WriteAsJsonAsync(response, options: JsonSerializerOptions);
    }

    private static ILogger GetHealthCheckLogger(WebApplication app)
    {
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();

        return loggerFactory.CreateLogger(HealthCheckLogName);
    }
}
