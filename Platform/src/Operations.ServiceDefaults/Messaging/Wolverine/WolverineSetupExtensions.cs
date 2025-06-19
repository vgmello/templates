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

        services
            .ConfigureOptions<ServiceBusOptions.Configurator>()
            .AddOptions<ServiceBusOptions>()
            .BindConfiguration(ServiceBusOptions.SectionName)
            .ValidateOnStart();

        var serviceBusOptions = configuration.GetSection(ServiceBusOptions.SectionName).Get<ServiceBusOptions>() ?? new ServiceBusOptions();
        var connectionString = configuration.GetConnectionString(ServiceBusOptions.SectionName);

        services.AddWolverine(ExtensionDiscovery.ManualOnly, opts =>
        {
            opts.ApplicationAssembly = Extensions.EntryAssembly;
            opts.ServiceName = serviceBusOptions.ServiceName;

            opts.UseSystemTextJsonForSerialization();

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                opts.ConfigurePostgresql(connectionString);
                opts.ConfigureReliableMessaging();
            }

            opts.Policies.Add<ExceptionHandlingPolicy>();
            opts.Policies.Add<FluentValidationPolicy>();

            opts.Policies.AddMiddleware(typeof(RequestPerformanceMiddleware));
            opts.Policies.AddMiddleware(typeof(OpenTelemetryInstrumentationMiddleware));

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
        var persistenceSchema = options.ServiceName.ToLowerInvariant();

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
