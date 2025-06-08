// Copyright (c) ABCDEG. All rights reserved.

using JasperFx.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Operations.ServiceDefaults.Messaging.Middlewares;
using Wolverine;
using Wolverine.Postgresql;
using Wolverine.Runtime;

namespace Operations.ServiceDefaults.Messaging.Wolverine;

public static class WolverineSetupExtensions
{
    public static IHostApplicationBuilder AddWolverine(this IHostApplicationBuilder builder, Action<WolverineOptions>? configure = null)
    {
        var wolverineRegistered = builder.Services.Any(s => s.ServiceType == typeof(IWolverineRuntime));

        if (wolverineRegistered)
            return builder;

        builder.Services
            .AddOptions<ServiceBusOptions>()
            .BindConfiguration(ServiceBusOptions.SectionName)
            .ValidateOnStart();

        builder.Services.ConfigureOptions<ServiceBusOptions.Configurator>();

        var serviceBusOptions = builder.Configuration
            .GetSection(ServiceBusOptions.SectionName).Get<ServiceBusOptions>() ?? new ServiceBusOptions();

        builder.Services.AddWolverine(ExtensionDiscovery.ManualOnly, opts =>
        {
            opts.ApplicationAssembly = Extensions.EntryAssembly;
            opts.ServiceName = serviceBusOptions.ServiceName;

            opts.UseSystemTextJsonForSerialization();
            opts.ConfigureAppHandlers();

            if (!string.IsNullOrWhiteSpace(serviceBusOptions.ConnectionString))
            {
                opts.ConfigurePostgresql(serviceBusOptions.ConnectionString);
                opts.ConfigureReliableMessaging();
            }

            opts.Policies.AddMiddleware(typeof(RequestPerformanceMiddleware));
            opts.Policies.AddMiddleware(typeof(OpenTelemetryInstrumentationMiddleware));
            opts.Policies.Add<FluentValidationPolicy>();

            configure?.Invoke(opts);

            opts.Services.AddResourceSetupOnStartup();
        });

        return builder;
    }

    public static WolverineOptions ConfigureAppHandlers(this WolverineOptions options)
    {
        var handlerAssemblies = DomainAssemblyAttribute.GetDomainAssemblies();

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
