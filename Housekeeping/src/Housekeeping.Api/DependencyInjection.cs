// Copyright (c) ABCDEG. All rights reserved.

using Dapper;
using Housekeeping.Contracts.Rooms.IntegrationEvents;
using Operations.ServiceDefaults.Messaging.Wolverine;
using Wolverine.Kafka;

namespace Housekeeping.Api;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        builder.AddNpgsqlDataSource("HousekeepingDb");

        return builder;
    }
}
