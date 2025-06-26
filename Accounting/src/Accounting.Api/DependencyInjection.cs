// Copyright (c) ABCDEG. All rights reserved.

using Operations.ServiceDefaults.Messaging.Kafka;
using Operations.ServiceDefaults.Messaging.Wolverine;
using Wolverine.Kafka;

namespace Accounting.Api;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddWolverine(opts =>
        {
            opts.ConfigureKafkaPublishing();
        });

        return builder;
    }

    private static void ConfigureKafkaPublishing(this WolverineOptions opts)
    {
        // Publish Ledger events to Kafka
        opts.PublishMessage<Accounting.Contracts.Ledgers.IntegrationEvents.LedgerCreated>()
            .ToKafkaTopic(KafkaTopicNamingConvention.Accounting.Ledger.Topic);

        opts.PublishMessage<Accounting.Contracts.Ledgers.IntegrationEvents.LedgerUpdated>()
            .ToKafkaTopic(KafkaTopicNamingConvention.Accounting.Ledger.Topic);

        // Publish Operation events to Kafka
        opts.PublishMessage<Accounting.Contracts.Operations.IntegrationEvents.BusinessDayEnded>()
            .ToKafkaTopic(KafkaTopicNamingConvention.Accounting.Operation.Topic);
    }
}
