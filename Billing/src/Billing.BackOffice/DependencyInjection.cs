// Copyright (c) ABCDEG. All rights reserved.

using Operations.ServiceDefaults.Messaging.Kafka;
using Operations.ServiceDefaults.Messaging.Wolverine;
using Wolverine.Kafka;

namespace Billing.BackOffice;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        builder.AddNpgsqlDataSource("BillingDb");

        builder.AddWolverine(opts =>
        {
            opts.ConfigureKafkaSubscriptions();
        });

        return builder;
    }

    private static void ConfigureKafkaSubscriptions(this WolverineOptions opts)
    {
        // Subscribe to Accounting Ledger events
        opts.ListenToKafkaTopic(KafkaTopicNamingConvention.Accounting.Ledger.Topic)
            .ProcessInline();

        // Subscribe to Accounting Operation events
        opts.ListenToKafkaTopic(KafkaTopicNamingConvention.Accounting.Operation.Topic)
            .ProcessInline();
    }
}
