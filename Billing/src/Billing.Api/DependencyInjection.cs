// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.Extensions.Hosting;
using Dapper;

namespace Billing.Api;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDataSource("BillingDb");

        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        return builder;
    }
}
