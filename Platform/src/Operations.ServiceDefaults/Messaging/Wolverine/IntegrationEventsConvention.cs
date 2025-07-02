// Copyright (c) ABCDEG. All rights reserved.

using JasperFx.Core.Reflection;
using Microsoft.Extensions.Hosting;
using Operations.Extensions.Abstractions.Messaging;
using Serilog;
using System.Linq.Expressions;
using System.Reflection;
using Wolverine;
using Wolverine.Kafka;

namespace Operations.ServiceDefaults.Messaging.Wolverine;

public static class IntegrationEventsExtensions
{
#pragma warning disable S3011
    private const BindingFlags PrivateStaticBindingFlags = BindingFlags.NonPublic | BindingFlags.Static;

    private static readonly MethodInfo SetupKafkaRouteMethodInfo =
        typeof(IntegrationEventsExtensions).GetMethod(nameof(SetupKafkaRoute), PrivateStaticBindingFlags)!;

    private static readonly MethodInfo CreatePartitionKeyGetterMethodInfo =
        typeof(IntegrationEventsExtensions).GetMethod(nameof(CreatePartitionKeyGetter), PrivateStaticBindingFlags)!;

    public static WolverineOptions SetupIntegrationEvents(this WolverineOptions opts, IHostEnvironment env)
    {
        var assemblies = DomainAssemblyAttribute.GetDomainAssemblies().Concat([Extensions.EntryAssembly]);

        var integrationEventTypes = assemblies.SelectMany(a => a.GetTypes()).Where(IsIntegrationEventType);

        foreach (var messageType in integrationEventTypes)
        {
            var topicAttribute = messageType.GetAttribute<EventTopicAttribute>();

            if (topicAttribute is null)
            {
                Log.Logger.Warning("IntegrationEvent {IntegrationEventType} does not have an EventTopicAttribute", messageType.Name);

                continue;
            }

            var topicName = GetTopicName(messageType, topicAttribute, env);

            var partitionKeyPropertyInfo = messageType.GetProperties()
                .FirstOrDefault(p => p.GetAttribute<PartitionKeyAttribute>() is not null);

            var partitionKeyGetter = partitionKeyPropertyInfo is not null
                ? CreatePartitionKeyGetterMethodInfo.MakeGenericMethod(messageType).Invoke(null, [partitionKeyPropertyInfo])
                : null;

            var setupKafkaRouteMethodInfo = SetupKafkaRouteMethodInfo.MakeGenericMethod(messageType);

            setupKafkaRouteMethodInfo.Invoke(null, [opts, topicName, partitionKeyGetter]);
        }

        return opts;
    }

    private static void SetupKafkaRoute<TEventType>(WolverineOptions opts, string topicName, Func<TEventType, string>? partitionKeyGetter)
    {
        opts.PublishMessage<TEventType>()
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

    private static string GetTopicName(Type messageType, EventTopicAttribute topicAttribute, IHostEnvironment env)
    {
        var envName = env.EnvironmentName switch
        {
            "Production" => "prod",
            "Test" => "test",
            _ => "dev"
        };

        var domainName = topicAttribute.Domain ?? messageType.Assembly.GetAttribute<DefaultDomainAttribute>()!.Domain;

        return $"{envName}.{domainName}.{topicAttribute.Topic}";
    }

    private static bool IsIntegrationEventType(Type messageType) => messageType.Namespace?.EndsWith("IntegrationEvents") == true;
}
