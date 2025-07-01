// Copyright (c) ABCDEG. All rights reserved.

using JasperFx.Core.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Operations.Extensions.Abstractions.Messaging;
using System.Collections.Concurrent;
using Wolverine.Configuration;
using Wolverine.Kafka.Internals;
using Wolverine.Runtime;
using Wolverine.Runtime.Routing;

namespace Operations.ServiceDefaults.Messaging.Wolverine;

public class IntegrationEventsConvention : IMessageRoutingConvention
{
    private readonly ConcurrentDictionary<Type, string> _topicCache = [];

    public void DiscoverListeners(IWolverineRuntime runtime, IReadOnlyList<Type> handledMessageTypes)
    {
        // Not worrying about this at all for this case
    }

    public IEnumerable<Endpoint> DiscoverSenders(Type messageType, IWolverineRuntime runtime)
    {
        if (!IsIntegrationEventType(messageType))
        {
            yield break;
        }

        if (_topicCache.TryGetValue(messageType, out var topicName))
        {
            yield return GetEndpoint(runtime, topicName);
        }

        topicName = CreateTopicName(runtime, messageType);

        if (topicName is null)
        {
            yield break;
        }

        _topicCache[messageType] = topicName;

        yield return GetEndpoint(runtime, topicName);
    }

    private static string? CreateTopicName(IWolverineRuntime runtime, Type messageType)
    {
        var topicAttribute = messageType.GetAttribute<EventTopicAttribute>();

        if (topicAttribute is null)
        {
            runtime.Logger.LogWarning("IntegrationEvent {IntegrationEventType} does not have an EventTopicAttribute", messageType.Name);

            return null;
        }

        var envName = runtime.Services.GetRequiredService<IHostEnvironment>().EnvironmentName switch
        {
            "Production" => "prod",
            "Test" => "test",
            _ => "dev"
        };

        var domainName = topicAttribute.Domain ?? messageType.Assembly.GetAttribute<DefaultDomainAttribute>()!.Domain;

        return $"{envName}.{domainName}.{topicAttribute.Topic}";
    }

    private static Endpoint GetEndpoint(IWolverineRuntime runtime, string topicName)
    {
        //TODO: Review this, it will probably not work, do something with the topic name also if this message has a local handler,
        //it should also be redirected to the local handler, but async (background) not in the same thread, so go to the local queue,
        // evaluate if needs to be done here or in the main config

        var kafkaTransport = runtime.Options.Transports.GetOrCreate<KafkaTransport>();

        var endpoints = kafkaTransport.Endpoints().ToList();

        return endpoints[0];
    }

    private static bool IsIntegrationEventType(Type messageType) => messageType.Namespace?.EndsWith("IntegrationEvents") == true;
}
