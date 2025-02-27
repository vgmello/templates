// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.Extensions.Hosting;
using Serilog;

namespace Operations.ServiceDefaults.Logging;

public static class LoggingExtensions
{
    public static IHostApplicationBuilder AddLogging(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSerilog((services, lc) => lc
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .WriteTo.OpenTelemetry()
            .Enrich.FromLogContext());

        return builder;
    }
}
