// Copyright (c) ABCDEG. All rights reserved.

using Dapper;

namespace Billing.BackOffice;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        builder.AddNpgsqlDataSource("BillingDb");

        return builder;
    }
}
