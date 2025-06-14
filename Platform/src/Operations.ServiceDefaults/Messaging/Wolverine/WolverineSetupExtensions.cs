// Copyright (c) ABCDEG. All rights reserved.

using JasperFx.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Operations.ServiceDefaults.Messaging.Middlewares;
using System.Reflection;
using Wolverine;
using Wolverine.Postgresql;
using Wolverine.Runtime;
using Wolverine.Transports.Kafka; // Added for UseKafka
using System; // Added for Console.WriteLine

namespace Operations.ServiceDefaults.Messaging.Wolverine;

public static class WolverineSetupExtensions
{
    public static bool SkipServiceRegistration { get; set; }

    public static IHostApplicationBuilder AddWolverine(this IHostApplicationBuilder builder, Action<WolverineOptions>? configure = null)
    {
        if (!SkipServiceRegistration)
        {
            builder.AddKeyedNpgsqlDataSource("ServiceBus");

            AddWolverineWithDefaults(builder.Services, builder.Configuration, configure);
        }

        return builder;
    }

    public static void AddWolverineWithDefaults(
        this IServiceCollection services, IConfiguration configuration, Action<WolverineOptions>? configure)
    {
        var wolverineRegistered = services.Any(s => s.ServiceType == typeof(IWolverineRuntime));

        if (wolverineRegistered)
            return;

        // Removed ServiceBusOptions related setup

        services.AddWolverine(ExtensionDiscovery.ManualOnly, opts =>
        {
            opts.ApplicationAssembly = Extensions.EntryAssembly;

            // Centralized ServiceName configuration
            string serviceNameForConfig = configuration["ServiceName"] ??
                                          (opts.ApplicationAssembly?.GetName().Name) ??
                                          "wolverine_service";
            opts.ServiceName = serviceNameForConfig.ToLowerInvariant().Replace(".", "-");
            Console.WriteLine($"Wolverine ServiceName configured as: {opts.ServiceName}");

            opts.UseSystemTextJsonForSerialization(); // Ensure this is present

            // Centralized PostgreSQL Configuration
            var pgConnectionString = configuration.GetConnectionString("servicedb");
            var pgSchemaName = $"{opts.ServiceName}_wolverine"; // Use already processed opts.ServiceName

            if (string.IsNullOrEmpty(pgConnectionString))
            {
                Console.WriteLine($"Warning: PostgreSQL connection string 'servicedb' for service '{opts.ServiceName}' not found. Wolverine PostgreSQL persistence might not be fully configured.");
            }
            else
            {
                opts.ConfigurePostgresql(pgConnectionString, pgSchemaName); // Pass schemaName
                Console.WriteLine($"Wolverine configured with PostgreSQL persistence for service '{opts.ServiceName}' using schema '{pgSchemaName}'.");
            }

            // Centralized Kafka Configuration
            var kafkaConnectionString = configuration.GetConnectionString("kafka");
            if (string.IsNullOrEmpty(kafkaConnectionString))
            {
                Console.WriteLine($"Warning: Kafka connection string 'kafka' for service '{opts.ServiceName}' not found. Wolverine Kafka transport might not be fully configured.");
            }
            else
            {
                opts.UseKafka(kafkaConnectionString); // Basic Kafka setup
                Console.WriteLine($"Wolverine configured with Kafka transport for service '{opts.ServiceName}'.");

                // Namespace-Based Routing for Integration Events to Kafka
                opts.PublishMessagesToKafka(message =>
                {
                    var messageNamespace = message.GetType().Namespace;
                    bool isIntegrationEvent = messageNamespace?.EndsWith(".IntegrationEvents") ?? false;
                    return isIntegrationEvent;
                });
                Console.WriteLine("Wolverine: Kafka publishing rule for *.IntegrationEvents messages configured.");
            }

            // Register CloudEventWrappingPolicy
            opts.Policies.Add<CloudEventWrappingPolicy>();
            Console.WriteLine("Wolverine: CloudEventWrappingPolicy registered.");

            // Common Policies
            opts.Policies.AddMiddleware(typeof(RequestPerformanceMiddleware));
            opts.Policies.AddMiddleware(typeof(OpenTelemetryInstrumentationMiddleware));
            opts.Policies.Add<FluentValidationPolicy>();

            // Reliable messaging (transactions, durable inbox/outbox for configured transports)
            opts.ConfigureReliableMessaging(); // Call this after transports are configured

            // Application-specific handlers are configured first.
            opts.ConfigureAppHandlers(opts.ApplicationAssembly);

            // Configure custom routing for local messages after handlers are discovered.
            ConfigureLocalMessageRouting(opts);

            // Allow for further customization by the application.
            configure?.Invoke(opts);

            opts.Services.AddResourceSetupOnStartup();
        });
    }

    private static void ConfigureLocalMessageRouting(WolverineOptions options)
    {
        var localAssemblies = DomainAssemblyAttribute.GetDomainAssemblies(options.ApplicationAssembly).ToList();

        if (localAssemblies.Any())
        {
            options.LocalQueue("local-processing")
                .UseInMemoryPersistence()
                .ProcessInline(); // For Mediator-like behavior / synchronous processing

            // Iterate through all known message types and their handlers
            // This uses HandlerGraph which is populated by ConfigureAppHandlers
            if (options.HandlerGraph != null) // Ensure HandlerGraph is available
            {
                foreach (var chain in options.HandlerGraph)
                {
                    if (chain.MessageType == null || chain.Handlers == null || !chain.Handlers.Any())
                    {
                        continue;
                    }

                    // A message type is considered local if ALL of its handlers are defined in local assemblies.
                    var handlersAreAllLocal = chain.Handlers.All(h =>
                        h.HandlerType != null && localAssemblies.Contains(h.HandlerType.Assembly));

                    if (handlersAreAllLocal)
                    {
                        options.PublishMessage(chain.MessageType)
                            .ToLocal("local-processing");
                    }
                }
            }
        }
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

    public static WolverineOptions ConfigurePostgresql(this WolverineOptions options, string connectionString, string schemaName)
    {
        var persistenceSchema = schemaName; // Use the passed schemaName

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
            .PublishAllMessages() // This will act as a fallback for messages not routed to local-processing
            .ToPostgresqlQueue("outbound")
            .UseDurableOutbox(); // Apply durable outbox directly to the PostgreSQL endpoint

        options
            .ListenToPostgresqlQueue("inbound")
            .UseDurableInbox() // Explicitly use durable inbox for inbound messages
            .MaximumMessagesToReceive(50);

        return options;
    }

    public static WolverineOptions ConfigureReliableMessaging(this WolverineOptions options)
    {
        // Opt into using "auto" transaction middleware
        options.Policies.AutoApplyTransactions();
        // options.Policies.UseDurableLocalQueues(); // May not be needed if local queues are in-memory
        // options.Policies.UseDurableOutboxOnAllSendingEndpoints(); // Replaced by specific configuration on PostgreSQL endpoint

        // If you still want durable local queues for general use (not for local-processing), you can keep UseDurableLocalQueues().
        // For this specific change, local-processing is in-memory.
        // If other local queues are used and need durability, this should be re-evaluated.
        // For now, let's assume durable outbox/inbox is handled at the transport level for PostgreSQL.
        return options;
    }
}
