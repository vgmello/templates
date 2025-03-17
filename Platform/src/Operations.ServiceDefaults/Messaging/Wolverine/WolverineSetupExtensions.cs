// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Operations.ServiceDefaults.Messaging.Behaviors;
using Wolverine;
using Wolverine.FluentValidation;
using Wolverine.Postgresql;
using Wolverine.Runtime;

namespace Operations.ServiceDefaults.Messaging.Wolverine;

public static class WolverineSetupExtensions
{
    public static IHostApplicationBuilder AddWolverine(this IHostApplicationBuilder builder,
        Action<WolverineOptions>? configure = null)
    {
        var wolverineRegistered = builder.Services.Any(s => s.ServiceType == typeof(IWolverineRuntime));

        if (wolverineRegistered)
            return builder;

        var messageBusOpts = builder.Configuration.GetSection("Messaging").Get<MessageBusOptions>() ??
                             new MessageBusOptions();

        MessageBusOptions.Validate(messageBusOpts);

        return builder.UseWolverine(opts =>
        {
            opts.ApplicationAssembly = Extensions.EntryAssembly;
            opts.ServiceName = messageBusOpts.ServiceName;

            // opts.UseFluentValidation();
            opts.UseSystemTextJsonForSerialization();

            opts.ConfigureAppHandlers();

            if (!string.IsNullOrWhiteSpace(messageBusOpts.ConnectionString))
            {
                opts.ConfigurePostgresql(messageBusOpts.ConnectionString);
                opts.ConfigureReliableMessaging();
            }

            opts.Policies.AddMiddleware(typeof(RequestPerformanceBehavior));

            configure?.Invoke(opts);
        });
    }

    public static WolverineOptions ConfigureAppHandlers(this WolverineOptions options)
    {
        var handlerAssemblies = DomainAssemblyAttribute
            .GetDomainAssemblies();

        foreach (var handlerAssembly in handlerAssemblies)
        {
            options.Discovery.IncludeAssembly(handlerAssembly);
        }

        return options;
    }

    public static WolverineOptions ConfigurePostgresql(this WolverineOptions options, string connectionString)
    {
        var persistenceSchema = options.ServiceName.ToLowerInvariant();

        options.UsePostgresqlPersistenceAndTransport(
                connectionString: connectionString,
                schema: persistenceSchema,
                transportSchema: "queues"
            )
            .AutoProvision();

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
