// Copyright (c) ABCDEG. All rights reserved.

namespace Billing.Api;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDataSource("BillingDb");

        return builder;
    }
}
