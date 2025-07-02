// Copyright (c) ABCDEG. All rights reserved.

using Billing.Contracts.Cashiers.IntegrationEvents;
using Dapper;
using Operations.ServiceDefaults.Messaging.Kafka;
using Operations.ServiceDefaults.Messaging.Wolverine;
using Wolverine.Kafka;

namespace Billing.Api;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        builder.AddNpgsqlDataSource("BillingDb");

        builder.AddWolverine(opts =>
        {
            opts.ConfigureKafkaPublishing();
        });

        return builder;
    }

    private static void ConfigureKafkaPublishing(this WolverineOptions opts)
    {
        // Publish Cashier events to Kafka
        opts.PublishMessage<CashierUpdated>()
            .ToKafkaTopic(KafkaTopicNamingConvention.Billing.Cashier.Topic);

        // Publish Invoice events to Kafka
        opts.PublishMessage<Billing.Contracts.Invoices.IntegrationEvents.InvoicePaid>()
            .ToKafkaTopic(KafkaTopicNamingConvention.Billing.Invoice.Topic);

        opts.PublishMessage<Billing.Contracts.Invoices.IntegrationEvents.InvoiceFinalized>()
            .ToKafkaTopic(KafkaTopicNamingConvention.Billing.Invoice.Topic);
    }
}
