// Copyright (c) ABCDEG. All rights reserved.

using JasperFx.Core.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Operations.Extensions.Abstractions.Messaging;
using Operations.ServiceDefaults.Extensions;
using Operations.ServiceDefaults.Messaging.Kafka;
using System.Reflection;
using Wolverine;
using Wolverine.Postgresql;

namespace Operations.ServiceDefaults.Messaging.Wolverine;

/// <summary>
///     Wolverine extension for configuring local routing of integration events within the same domain.
/// </summary>
/// <remarks>
///     This extension optimizes message routing by using PostgreSQL queues for integration events
///     when both the publisher and handler exist within the same application domain.
///     Cross-domain events continue to use Kafka for distributed messaging.
/// </remarks>
public class LocalIntegrationEventsExtensions(
    ILogger<LocalIntegrationEventsExtensions> logger,
    IHostEnvironment environment) : IWolverineExtension
{
    private static readonly MethodInfo SetupLocalRouteMethodInfo =
        typeof(LocalIntegrationEventsExtensions).GetMethod(nameof(SetupLocalRoute), BindingFlags.NonPublic | BindingFlags.Static)!;

    /// <summary>
    ///     Configures local routing for integration events within the same domain.
    /// </summary>
    /// <remarks>
    ///     Features:
    ///     <list type="bullet">
    ///         <item>Discovers integration events with handlers in the same application</item>
    ///         <item>Configures PostgreSQL queue routing for same-domain events</item>
    ///         <item>Generates queue names based on domain and topic</item>
    ///         <item>Preserves Kafka routing for cross-domain events</item>
    ///         <item>Provides durable messaging within the same domain</item>
    ///     </list>
    /// </remarks>
    public void Configure(WolverineOptions options)
    {
        var integrationEventTypesWithHandlers = IntegrationEventsDiscovery.GetIntegrationEventTypesWithHandlers();
        var localDomain = GetLocalDomain();
        var routedEventTypes = new HashSet<string>();

        foreach (var messageType in integrationEventTypesWithHandlers)
        {
            var topicAttribute = messageType.GetAttribute<EventTopicAttribute>();

            if (topicAttribute is null)
            {
                logger.LogWarning("IntegrationEvent {IntegrationEventType} does not have an EventTopicAttribute", messageType.Name);

                continue;
            }

            var eventDomain = GetEventDomain(messageType, topicAttribute);

            // Only route locally if the event domain matches the local domain
            if (!string.Equals(eventDomain, localDomain, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogDebug(
                    "Skipping local routing for {EventType} as it belongs to domain {EventDomain}, not local domain {LocalDomain}",
                    messageType.Name, eventDomain, localDomain);

                continue;
            }

            var queueName = GetLocalQueueName(messageType, topicAttribute, eventDomain);
            var setupLocalRouteMethod = SetupLocalRouteMethodInfo.MakeGenericMethod(messageType);

            setupLocalRouteMethod.Invoke(null, [options, queueName]);
            routedEventTypes.Add(messageType.Name);

            logger.LogDebug("Configured local routing for {EventType} to PostgreSQL queue {QueueName}", messageType.Name, queueName);
        }

        if (routedEventTypes.Count > 0)
        {
            logger.LogInformation("Configured local routing for {EventCount} integration events in domain {Domain} using PostgreSQL queues",
                routedEventTypes.Count, localDomain);
        }
    }

    private static void SetupLocalRoute<TEventType>(WolverineOptions options, string queueName)
    {
        // Configure publisher to route to PostgreSQL queue
        options
            .PublishMessage<TEventType>()
            .ToPostgresqlQueue(queueName);

        // Configure listener for the PostgreSQL queue
        options
            .ListenToPostgresqlQueue(queueName)
            .MaximumMessagesToReceive(50);
    }

    /// <summary>
    ///     Determines the local domain from the entry assembly.
    /// </summary>
    private string GetLocalDomain()
    {
        var entryAssembly = ServiceDefaultsExtensions.EntryAssembly;
        var defaultDomainAttribute = entryAssembly.GetAttribute<DefaultDomainAttribute>();

        if (defaultDomainAttribute?.Domain is not null)
        {
            return defaultDomainAttribute.Domain;
        }

        // Fallback to assembly name prefix
        var assemblyName = entryAssembly.GetName().Name ?? "Unknown";
        var dotIndex = assemblyName.IndexOf('.');

        return dotIndex > 0 ? assemblyName[..dotIndex] : assemblyName;
    }

    /// <summary>
    ///     Gets the domain for an event type based on its attribute or assembly default.
    /// </summary>
    private string GetEventDomain(Type messageType, EventTopicAttribute topicAttribute)
    {
        if (!string.IsNullOrWhiteSpace(topicAttribute.Domain))
        {
            return topicAttribute.Domain;
        }

        var defaultDomainAttribute = messageType.Assembly.GetAttribute<DefaultDomainAttribute>();

        if (defaultDomainAttribute?.Domain is not null)
        {
            return defaultDomainAttribute.Domain;
        }

        // Fallback to assembly name prefix
        var assemblyName = messageType.Assembly.GetName().Name ?? "Unknown";
        var dotIndex = assemblyName.IndexOf('.');

        return dotIndex > 0 ? assemblyName[..dotIndex] : assemblyName;
    }

    /// <summary>
    ///     Generates a PostgreSQL queue name for the event.
    /// </summary>
    private string GetLocalQueueName(Type messageType, EventTopicAttribute topicAttribute, string domain)
    {
        var topicName = topicAttribute.ShouldPluralizeTopicName
            ? topicAttribute.Topic.Pluralize()
            : topicAttribute.Topic;

        var scope = topicAttribute.Internal ? "internal" : "public";

        // Format: local-{domain}-{scope}-{topic}
        // Note: PostgreSQL queue names should use hyphens instead of dots
        return $"local-{domain}-{scope}-{topicName}".ToLowerInvariant().Replace(".", "-");
    }
}
