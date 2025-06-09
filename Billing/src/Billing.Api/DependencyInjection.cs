// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.Extensions.Hosting;
using Dapper;

namespace Billing.Api;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDataSource("BillingDb");

        DefaultTypeMap.MatchNamesWithUnderscores = true;

        // Add health checks for the billing database
        builder.Services.AddHealthChecks()
            .AddNpgSql(
                builder.Configuration.GetConnectionString("BillingDb")!,
                name: "billing-db",
                tags: ["ready"])
            .AddNpgSql(
                builder.Configuration.GetConnectionString("ServiceBusDb") ??
                builder.Configuration.GetConnectionString("ServiceBus__ConnectionString")!,
                name: "servicebus-db",
                tags: ["ready"]);

        return builder;
    }
}
