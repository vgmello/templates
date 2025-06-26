// Copyright (c) ABCDEG. All rights reserved.

namespace Accounting.BackOffice.Orleans;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        builder.AddNpgsqlDataSource("AccountingDb");

        return builder;
    }
}
