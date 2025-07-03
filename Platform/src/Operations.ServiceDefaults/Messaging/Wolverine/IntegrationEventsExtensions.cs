// Copyright (c) ABCDEG. All rights reserved.

using CloudNative.CloudEvents;
using JasperFx.Core.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Operations.Extensions.Abstractions.Messaging;
using Serilog;
using System.Linq.Expressions;
using System.Reflection;
using Wolverine;
using Wolverine.Kafka;

#pragma warning disable S3011

namespace Operations.ServiceDefaults.Messaging.Wolverine;

/// <summary>
///     Wolverine extension for configuring integration events with Kafka.
/// </summary>
public class IntegrationEventsExtensions(IOptions<ServiceBusOptions> serviceBusOptions, IHostEnvironment environment) : IWolverineExtension
{
    private const string IntegrationEventsNamespace = ".IntegrationEvents";

    private const BindingFlags PrivateStaticBindingFlags = BindingFlags.NonPublic | BindingFlags.Static;

    private static readonly MethodInfo SetupKafkaRouteMethodInfo =
        typeof(IntegrationEventsExtensions).GetMethod(nameof(SetupKafkaRoute), PrivateStaticBindingFlags)!;

    private static readonly MethodInfo CreatePartitionKeyGetterMethodInfo =
        typeof(IntegrationEventsExtensions).GetMethod(nameof(CreatePartitionKeyGetter), PrivateStaticBindingFlags)!;

    /// <summary>
    ///     Sets up integration event routing for Kafka based on event attributes.
    /// </summary>
    /// <remarks>
    ///     Features:
    ///     <list type="bullet">
    ///         <item>Discovers all integration event types from domain assemblies</item>
    ///         <item>Configures Kafka topic routing based on EventTopicAttribute</item>
    ///         <item>Sets up partition key routing using PartitionKeyAttribute or IIntegrationEvent.GetPartitionKey()</item>
    ///         <item>Generates environment-specific topic names (dev, test, prod)</item>
    ///     </list>
    ///     Integration events are identified by:
    ///     <list type="bullet">
    ///         <item>Having their namespace end with "IntegrationEvents"</item>
    ///         <item>Being decorated with EventTopicAttribute</item>
    ///     </list>
    /// </remarks>
    public void Configure(WolverineOptions options)
    {
        var integrationEventTypes = GetIntegrationEventTypes();

        foreach (var messageType in integrationEventTypes)
        {
            var topicAttribute = messageType.GetAttribute<EventTopicAttribute>();

            if (topicAttribute is null)
            {
                Log.Logger.Warning("IntegrationEvent {IntegrationEventType} does not have an EventTopicAttribute", messageType.Name);

                continue;
            }

            var topicName = GetTopicName(messageType, topicAttribute, environment.EnvironmentName);

            var partitionKeyPropertyInfo = messageType.GetProperties()
                .FirstOrDefault(p => p.GetAttribute<PartitionKeyAttribute>() is not null);

            var partitionKeyGetter = partitionKeyPropertyInfo is not null
                ? CreatePartitionKeyGetterMethodInfo.MakeGenericMethod(messageType).Invoke(null, [partitionKeyPropertyInfo])
                : null;

            var setupKafkaRouteMethodInfo = SetupKafkaRouteMethodInfo.MakeGenericMethod(messageType);

            setupKafkaRouteMethodInfo.Invoke(null, [options, serviceBusOptions.Value, topicName, partitionKeyGetter]);
        }
    }

    private static void SetupKafkaRoute<TEventType>(
        WolverineOptions wolverineOptions,
        ServiceBusOptions serviceBusOptions,
        string topicName,
        Func<TEventType, string>? partitionKeyGetter)
    {
        wolverineOptions.PublishMessage<TEventType>()
            .ToKafkaTopic(topicName)
            .CustomizeOutgoing(e =>
            {
                if (e.Message is IIntegrationEvent integrationEvent)
                {
                    e.PartitionKey = integrationEvent.GetPartitionKey();
                }
                else if (e.Message is not null && partitionKeyGetter is not null)
                {
                    e.PartitionKey = partitionKeyGetter((TEventType)e.Message);
                }

                var messageType = e.Message?.GetType();

                var cloudEvent = new CloudEvent
                {
                    Id = Guid.CreateVersion7().ToString(),
                    Type = messageType?.Name ?? typeof(TEventType).Name,
                    Source = serviceBusOptions.ServiceUrn,
                    Time = DateTimeOffset.UtcNow,
                    Data = e.Message,
                    DataContentType = "application/json"
                };

                e.Message = cloudEvent;
                e.ContentType = "application/cloudevents+json";
            });
    }

    /// <summary>
    ///     Creates a compiled expression that efficiently retrieves a partition key property value as a string.
    /// </summary>
    /// <typeparam name="T">The type of the object containing the partition key property.</typeparam>
    /// <param name="partitionKeyPropertyInfo">
    ///     The <see cref="PropertyInfo" /> representing the property to be used as the partition key.
    ///     This property should be decorated with a PartitionKeyAttribute or similar identifier.
    /// </param>
    /// <returns>
    ///     A compiled <see cref="Func{T, TResult}" /> that takes an instance of type <typeparamref name="T" />
    ///     and returns the partition key property value as a string by calling ToString() on the property value.
    /// </returns>
    /// <remarks>
    ///     This method is called on every event published, therefore performance is important, that is why I'm using expression trees
    ///     instead of reflection.
    ///     <para>
    ///         Generated Code:
    ///         <code>instance => {partitionKeyProperty}.ToString()</code>
    ///     </para>
    /// </remarks>
    private static Func<T, string> CreatePartitionKeyGetter<T>(PropertyInfo partitionKeyPropertyInfo)
    {
        var parameter = Expression.Parameter(typeof(T), "instance");
        var propertyAccessor = Expression.Property(parameter, partitionKeyPropertyInfo);

        var toString = typeof(object).GetMethod(nameof(ToString));
        var toStringCall = Expression.Call(propertyAccessor, toString!);

        var lambda = Expression.Lambda<Func<T, string>>(toStringCall, parameter);

        return lambda.Compile();
    }

    private static IEnumerable<Type> GetIntegrationEventTypes()
    {
        Assembly[] appAssemblies = [..DomainAssemblyAttribute.GetDomainAssemblies(), Extensions.EntryAssembly];

        var domainPrefixes = appAssemblies
            .Select(a => a.GetName().Name)
            .Where(assemblyName => assemblyName is not null)
            .Select(assemblyName =>
            {
                var mainNamespaceIndex = assemblyName!.IndexOf('.');

                return mainNamespaceIndex >= 0 ? assemblyName[..mainNamespaceIndex] : assemblyName;
            })
            .ToHashSet();

        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly =>
            {
                var name = assembly.GetName().Name;

                return name is not null && domainPrefixes.Any(prefix => name.StartsWith(prefix));
            })
            .ToArray();

        return assemblies.SelectMany(a => a.GetTypes()).Where(IsIntegrationEventType);
    }

    /// <summary>
    ///     Generates a fully qualified topic name based on environment and domain.
    /// </summary>
    /// <param name="messageType">The integration event type.</param>
    /// <param name="topicAttribute">The event topic attribute.</param>
    /// <param name="env">Environment name.</param>
    /// <returns>A topic name in the format: {env}.{domain}.{topic}[.{version}]</returns>
    private static string GetTopicName(Type messageType, EventTopicAttribute topicAttribute, string env)
    {
        var envName = env switch
        {
            "Production" => "prod",
            "Test" => "test",
            _ => "dev"
        };

        var domainName = !string.IsNullOrWhiteSpace(topicAttribute.Domain)
            ? topicAttribute.Domain
            : messageType.Assembly.GetAttribute<DefaultDomainAttribute>()!.Domain;

        var versionSuffix = string.IsNullOrWhiteSpace(topicAttribute.Version) ? null : $".{topicAttribute.Version}";

        return $"{envName}.{domainName}.{topicAttribute.Topic}{versionSuffix}".ToLowerInvariant();
    }

    private static bool IsIntegrationEventType(Type messageType) => messageType.Namespace?.EndsWith(IntegrationEventsNamespace) == true;
}
