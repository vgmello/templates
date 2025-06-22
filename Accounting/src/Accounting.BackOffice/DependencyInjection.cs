// Copyright (c) ABCDEG. All rights reserved.

using Operations.ServiceDefaults.Messaging.Kafka;
using Operations.ServiceDefaults.Messaging.Wolverine;
using Wolverine.Kafka;

namespace Accounting.BackOffice;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        builder.AddNpgsqlDataSource("AccountingDb");

        builder.AddWolverine(opts =>
        {
            opts.ConfigureKafkaSubscriptions();
        });

        return builder;
    }

    private static void ConfigureKafkaSubscriptions(this WolverineOptions opts)
    {
        // Subscribe to Billing Invoice events
        opts.ListenToKafkaTopic(KafkaTopicNamingConvention.Billing.Invoice.Topic)
            .ProcessInline();

        // Subscribe to Billing Cashier events
        opts.ListenToKafkaTopic(KafkaTopicNamingConvention.Billing.Cashier.Topic)
            .ProcessInline();
    }
}
