// Copyright (c) ABCDEG. All rights reserved.

using Operations.ServiceDefaults.Messaging.Kafka;
using Operations.ServiceDefaults.Messaging.Wolverine;
using Wolverine.Kafka;

namespace Billing.Api;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
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
        opts.PublishMessage<Billing.Contracts.Cashier.IntegrationEvents.CashierCreatedEvent>()
            .ToKafkaTopic(KafkaTopicNamingConvention.Billing.Cashier.Topic);

        opts.PublishMessage<Billing.Contracts.Cashier.IntegrationEvents.CashierUpdatedEvent>()
            .ToKafkaTopic(KafkaTopicNamingConvention.Billing.Cashier.Topic);

        // Publish Invoice events to Kafka
        opts.PublishMessage<Billing.Contracts.Invoices.IntegrationEvents.InvoicePaidEvent>()
            .ToKafkaTopic(KafkaTopicNamingConvention.Billing.Invoice.Topic);

        opts.PublishMessage<Billing.Contracts.Invoices.IntegrationEvents.InvoiceFinalized>()
            .ToKafkaTopic(KafkaTopicNamingConvention.Billing.Invoice.Topic);
    }
}
