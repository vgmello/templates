// Copyright (c) ABCDEG. All rights reserved.

namespace Housekeeping.BackOffice.Orleans;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        builder.AddNpgsqlDataSource("HousekeepingDb");

        return builder;
    }
}