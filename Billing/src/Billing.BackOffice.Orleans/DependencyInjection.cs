// Copyright (c) ABCDEG. All rights reserved.

using Dapper;

namespace Billing.BackOffice.Orleans;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        builder.AddNpgsqlDataSource("BillingDb");

        builder.AddKeyedAzureTableClient("clustering");
        builder.AddKeyedAzureTableClient("grain-state");

        return builder;
    }
}
