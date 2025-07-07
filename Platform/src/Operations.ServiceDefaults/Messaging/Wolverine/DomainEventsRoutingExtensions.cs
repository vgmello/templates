// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Wolverine;
using Wolverine.Postgresql;

namespace Operations.ServiceDefaults.Messaging.Wolverine;

/// <summary>
///     Wolverine extension for automatically routing domain events within the application using PostgreSQL queues.
/// </summary>
/// <remarks>
///     This extension automatically discovers all domain events (messages in namespaces ending with ".DomainEvents")
///     and configures PostgreSQL queue routing for them. This enables reliable, durable processing of domain events
///     within the same application or service boundary using PostgreSQL transport.
/// </remarks>
public class DomainEventsRoutingExtensions(
    ILogger<DomainEventsRoutingExtensions> logger,
    IHostEnvironment environment) : IWolverineExtension
{
    private static readonly MethodInfo SetupDomainEventRouteMethodInfo =
        typeof(DomainEventsRoutingExtensions).GetMethod(nameof(SetupDomainEventRoute), BindingFlags.NonPublic | BindingFlags.Static)!;

    /// <summary>
    ///     Configures automatic PostgreSQL queue routing for domain events.
    /// </summary>
    /// <remarks>
    ///     Features:
    ///     <list type="bullet">
    ///         <item>Discovers all domain events from assemblies marked with DomainAssemblyAttribute</item>
    ///         <item>Configures PostgreSQL queue routing for domain events with handlers</item>
    ///         <item>Uses naming convention: domain-events-{domain}-{eventname}</item>
    ///         <item>Ensures durable processing within the application boundary</item>
    ///         <item>Separate from integration events (which use Kafka for cross-domain communication)</item>
    ///     </list>
    /// </remarks>
    public void Configure(WolverineOptions options)
    {
        var domainEventTypesWithHandlers = GetDomainEventTypesWithHandlers();
        var localDomain = GetLocalDomain();
        var routedEventTypes = new HashSet<string>();

        foreach (var messageType in domainEventTypesWithHandlers)
        {
            var queueName = GetDomainEventQueueName(messageType, localDomain);
            var setupDomainEventRouteMethod = SetupDomainEventRouteMethodInfo.MakeGenericMethod(messageType);

            setupDomainEventRouteMethod.Invoke(null, [options, queueName]);
            routedEventTypes.Add(messageType.Name);

            logger.LogDebug("Configured domain event routing for {EventType} to PostgreSQL queue {QueueName}",
                messageType.Name, queueName);
        }

        if (routedEventTypes.Count > 0)
        {
            logger.LogInformation(
                "Configured domain event routing for {EventCount} domain events in domain {Domain} using PostgreSQL queues",
                routedEventTypes.Count, localDomain);

            // Log each configured route for debugging
            foreach (var eventType in routedEventTypes)
            {
                logger.LogInformation("Domain event {EventType} routed to PostgreSQL queue", eventType);
            }

            // Write to file for debugging (development only)
            try
            {
                var debugInfo = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC] Domain Events Routing Configuration:\n" +
                                $"- Domain: {localDomain}\n" +
                                $"- Configured events: {routedEventTypes.Count}\n" +
                                string.Join("\n", routedEventTypes.Select(e => $"  * {e}")) + "\n\n";

                File.AppendAllText("wolverine-domain-events-routing.log", debugInfo);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to write debug information to file");
            }
        }
        else
        {
            logger.LogInformation("No domain events with handlers found for routing in domain {Domain}", localDomain);
        }
    }

    private static void SetupDomainEventRoute<TEventType>(WolverineOptions options, string queueName)
    {
        // Configure publisher to route to PostgreSQL queue
        options
            .PublishMessage<TEventType>()
            .ToPostgresqlQueue(queueName);

        // Configure listener for the PostgreSQL queue with optimized settings for domain events
        options
            .ListenToPostgresqlQueue(queueName)
            .MaximumMessagesToReceive(25) // Smaller batch size for domain events
            .Sequential(); // Process domain events sequentially to maintain order
    }

    /// <summary>
    ///     Discovers and retrieves domain event types that have handlers in the current application.
    /// </summary>
    private IEnumerable<Type> GetDomainEventTypesWithHandlers()
    {
        var handlerMethods = GetHandlerMethods();

        var domainEvents = handlerMethods.SelectMany(method =>
                method.GetParameters()
                    .Select(parameter => parameter.ParameterType)
                    .Where(IsDomainEventType))
            .ToHashSet();

        return domainEvents;
    }

    /// <summary>
    ///     Gets all handler methods from domain assemblies.
    /// </summary>
    private IEnumerable<MethodInfo> GetHandlerMethods()
    {
        Assembly[] handlerAssemblies = [..DomainAssemblyAttribute.GetDomainAssemblies(), ServiceDefaultsExtensions.EntryAssembly];

        var candidateHandlers = handlerAssemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsClass: true })
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static));

        return candidateHandlers.Where(IsHandlerMethod);
    }

    /// <summary>
    ///     Checks if a method is a Wolverine handler method.
    /// </summary>
    private static bool IsHandlerMethod(MethodInfo method)
    {
        var handlerMethodNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Handle", "HandleAsync", "Handles", "HandlesAsync",
            "Consume", "ConsumeAsync", "Consumes", "ConsumesAsync"
        };

        return handlerMethodNames.Contains(method.Name) && method.GetParameters().Length > 0;
    }

    /// <summary>
    ///     Checks if a type is a domain event (namespace ends with ".DomainEvents").
    /// </summary>
    private static bool IsDomainEventType(Type messageType) =>
        messageType.Namespace?.EndsWith(".DomainEvents", StringComparison.OrdinalIgnoreCase) == true;

    /// <summary>
    ///     Determines the local domain from the entry assembly.
    /// </summary>
    private string GetLocalDomain()
    {
        var entryAssembly = ServiceDefaultsExtensions.EntryAssembly;

        // Try to get domain from DefaultDomainAttribute first
        var defaultDomainAttribute =
            entryAssembly.GetCustomAttribute<Operations.Extensions.Abstractions.Messaging.DefaultDomainAttribute>();

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
    ///     Generates a PostgreSQL queue name for the domain event.
    /// </summary>
    private string GetDomainEventQueueName(Type messageType, string domain)
    {
        var eventName = messageType.Name.ToLowerInvariant();

        // Format: domain-events-{domain}-{eventname}
        return $"domain-events-{domain}-{eventName}".ToLowerInvariant().Replace(".", "-");
    }
}
