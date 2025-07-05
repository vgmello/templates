// Copyright (c) ABCDEG. All rights reserved.

using Operations.ServiceDefaults.Messaging.Wolverine;
using Wolverine.Kafka;

namespace Housekeeping.BackOffice;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        builder.AddNpgsqlDataSource("HousekeepingDb");

        return builder;
    }
}
