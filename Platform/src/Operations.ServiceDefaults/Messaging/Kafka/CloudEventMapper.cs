// Copyright (c) ABCDEG. All rights reserved.

using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Kafka;
using CloudNative.CloudEvents.SystemTextJson;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using Wolverine;
using Wolverine.Kafka;

namespace Operations.ServiceDefaults.Messaging.Kafka;

public class CloudEventMapper(IOptions<CloudEventsSettings> settings, IOptions<ServiceBusOptions> serviceBusOptions)
    : IKafkaEnvelopeMapper
{
    private readonly CloudEventFormatter _formatter = new JsonEventFormatter();
    private readonly CloudEventsSettings _settings = settings.Value;

    public void MapEnvelopeToOutgoing(Envelope envelope, Message<string, byte[]> outgoing)
    {
        var cloudEvent = new CloudEvent
        {
            Id = envelope.Id.ToString(),
            Source = serviceBusOptions.Value.ServiceUrn,
            Type = envelope.MessageType,
            Data = envelope.Message,
            DataContentType = "application/json",
            Time = DateTimeOffset.UtcNow
        };

        var parentId = Activity.Current?.ParentId ?? envelope.ParentId;

        if (parentId is not null)
            cloudEvent.SetTraceParent(parentId);

        var message = cloudEvent.ToKafkaMessage(_settings.Mode, _formatter);

        outgoing.Value = message.Value;
        outgoing.Key = envelope.PartitionKey ?? envelope.Id.ToString();

        if (_settings.Mode == ContentMode.Binary)
        {
            outgoing.Headers = message.Headers;
        }
    }

    public void MapIncomingToEnvelope(Envelope envelope, Message<string, byte[]> incoming)
    {
        if (incoming.Key is not null && incoming!.IsCloudEvent())
        {
            var cloudEvent = incoming!.ToCloudEvent(_formatter);

            envelope.Message = cloudEvent.Data;
            envelope.MessageType = cloudEvent.DataContentType;
        }
    }

    public IEnumerable<string> AllHeaders()
    {
        yield break;
    }
}
