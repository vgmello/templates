// Copyright (c) ABCDEG. All rights reserved.

using Dapper;
using Operations.ServiceDefaults.Messaging.Kafka;
using Operations.ServiceDefaults.Messaging.Wolverine;
using Wolverine.Kafka;

namespace Housekeeping.BackOffice;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        builder.AddNpgsqlDataSource("HousekeepingDb");

        builder.AddWolverine(opts =>
        {
            opts.ConfigureKafkaSubscriptions();
        });

        return builder;
    }

    private static void ConfigureKafkaSubscriptions(this WolverineOptions opts)
    {
        // Subscribe to Housekeeping Room events
        opts.ListenToKafkaTopic(KafkaTopicNamingConvention.Housekeeping.Room.Topic)
            .ProcessInline();
    }
}