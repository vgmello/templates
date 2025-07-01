// Copyright (c) ABCDEG. All rights reserved.

using Dapper;
using Housekeeping.Contracts.Rooms.IntegrationEvents;
using Operations.ServiceDefaults.Messaging.Kafka;
using Operations.ServiceDefaults.Messaging.Wolverine;
using Wolverine.Kafka;

namespace Housekeeping.Api;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        builder.AddNpgsqlDataSource("HousekeepingDb");

        builder.AddWolverine(opts =>
        {
            opts.ConfigureKafkaPublishing();
        });

        return builder;
    }

    private static void ConfigureKafkaPublishing(this WolverineOptions opts)
    {
        // Publish Room events to Kafka
        opts.PublishMessage<RoomStatusChanged>()
            .ToKafkaTopic(KafkaTopicNamingConvention.Housekeeping.Room.Topic);

        opts.PublishMessage<CleaningCompleted>()
            .ToKafkaTopic(KafkaTopicNamingConvention.Housekeeping.Room.Topic);

        opts.PublishMessage<MaintenanceRequested>()
            .ToKafkaTopic(KafkaTopicNamingConvention.Housekeeping.Room.Topic);

        opts.PublishMessage<MiniFridgeUpdated>()
            .ToKafkaTopic(KafkaTopicNamingConvention.Housekeeping.Room.Topic);
    }
}
