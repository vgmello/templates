// Copyright (c) ABCDEG. All rights reserved.

using JasperFx.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Operations.ServiceDefaults.Messaging.CloudEvents;
using Operations.ServiceDefaults.Messaging.Middlewares;
using System.Reflection;
using Wolverine;
using Wolverine.Kafka;
using Wolverine.Postgresql;
using Wolverine.Runtime;

namespace Operations.ServiceDefaults.Messaging.Wolverine;

public static class WolverineSetupExtensions
{
    public static bool SkipServiceRegistration { get; set; }

    public static IHostApplicationBuilder AddWolverine(this IHostApplicationBuilder builder, Action<WolverineOptions>? configure = null)
    {
        if (!SkipServiceRegistration)
        {
            builder.AddKeyedNpgsqlDataSource("ServiceBus");

            AddWolverineWithDefaults(builder.Services, builder.Environment, builder.Configuration, configure);
        }

        return builder;
    }

    public static void AddWolverineWithDefaults(
        this IServiceCollection services, IHostEnvironment env, IConfiguration configuration, Action<WolverineOptions>? configure)
    {
        var wolverineRegistered = services.Any(s => s.ServiceType == typeof(IWolverineRuntime));

        if (wolverineRegistered)
            return;

        services
            .ConfigureOptions<ServiceBusOptions.Configurator>()
            .AddOptions<ServiceBusOptions>()
            .BindConfiguration(ServiceBusOptions.SectionName)
            .ValidateOnStart();

        var connectionString = configuration.GetConnectionString(ServiceBusOptions.SectionName);

        services.AddWolverine(ExtensionDiscovery.ManualOnly, opts =>
        {
            opts.ApplicationAssembly = Extensions.EntryAssembly;
            opts.ServiceName = ServiceBusOptions.GetServiceName(env.ApplicationName);

            opts.UseSystemTextJsonForSerialization();

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                opts.ConfigurePostgresql(connectionString);
                opts.ConfigureReliableMessaging();
            }

            opts.Policies.Add<ExceptionHandlingPolicy>();
            opts.Policies.Add<FluentValidationPolicy>();

            opts.Policies.AddMiddleware<RequestPerformanceMiddleware>();
            opts.Policies.AddMiddleware(typeof(OpenTelemetryInstrumentationMiddleware));
            opts.Policies.AddMiddleware<CloudEventMiddleware>();

            var kafkaConnectionString = configuration.GetConnectionString("Messaging");

            if (!string.IsNullOrEmpty(kafkaConnectionString))
            {
                opts.UseKafka(kafkaConnectionString);

                // Add Kafka health check
                services.AddHealthChecks()
                    .AddKafka(options =>
                    {
                        options.BootstrapServers = kafkaConnectionString;
                    }, name: "kafka", tags: new[] { "messaging", "kafka" });
            }

            configure?.Invoke(opts);

            opts.ConfigureAppHandlers(opts.ApplicationAssembly);

            opts.Services.AddResourceSetupOnStartup();
        });
    }

    public static WolverineOptions ConfigureAppHandlers(this WolverineOptions options, Assembly? applicationAssembly = null)
    {
        var handlerAssemblies = DomainAssemblyAttribute.GetDomainAssemblies(applicationAssembly);

        foreach (var handlerAssembly in handlerAssemblies)
        {
            options.Discovery.IncludeAssembly(handlerAssembly);
        }

        return options;
    }

    public static WolverineOptions ConfigurePostgresql(this WolverineOptions options, string connectionString)
    {
        var persistenceSchema = options.ServiceName
            .Replace(".", "_")
            .Replace("-", "_")
            .ToLowerInvariant();

#pragma warning disable CS0618 // Remove when EnableMessageTransport is implemented by Wolverine
#pragma warning disable S125 // Remove when EnableMessageTransport is implemented by Wolverine

        options
            .UsePostgresqlPersistenceAndTransport(connectionString, schema: persistenceSchema, transportSchema: "queues")
            .AutoProvision();

        // Uncomment the following lines once EnableMessageTransport is fully implemented in Wolverine
        // options
        //     .PersistMessagesWithPostgresql(connectionString, schemaName: persistenceSchema)
        //     .EnableMessageTransport(transport =>
        //         transport
        //             .TransportSchemaName("queues")
        //             .AutoProvision());

#pragma warning restore S125
#pragma warning restore CS0618

        options
            .PublishAllMessages()
            .ToPostgresqlQueue("outbound");

        options
            .ListenToPostgresqlQueue("inbound")
            .MaximumMessagesToReceive(50);

        return options;
    }

    public static WolverineOptions ConfigureReliableMessaging(this WolverineOptions options)
    {
        // Opt into using "auto" transaction middleware
        options.Policies.AutoApplyTransactions();
        options.Policies.UseDurableLocalQueues();
        options.Policies.UseDurableOutboxOnAllSendingEndpoints();

        return options;
    }
}
