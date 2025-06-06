// Copyright (c) ABCDEG. All rights reserved.

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

        return builder.UseWolverine(opts =>
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
            opts.Policies.Add<FluentValidationPolicy>();

            configure?.Invoke(opts);
        });
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

        options
            .PersistMessagesWithPostgresql(
                connectionString: connectionString,
                schemaName: persistenceSchema
            )
            .EnableMessageTransport(transportCfg =>
                transportCfg
                    .TransportSchemaName("queues")
                    .AutoProvision()
            );

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
