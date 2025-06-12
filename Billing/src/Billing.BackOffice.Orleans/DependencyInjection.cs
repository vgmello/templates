// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.BackOffice.Orleans;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        builder.AddNpgsqlDataSource("BillingDb");

        return builder;
    }
}
