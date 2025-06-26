// Copyright (c) ABCDEG. All rights reserved.

using CloudNative.CloudEvents;
using Microsoft.Extensions.Options;
using Wolverine;

namespace Operations.ServiceDefaults.Messaging.CloudEvents;

public class CloudEventMiddleware(IOptions<ServiceBusOptions> serviceBusOptions)
{
    private const string IntegrationEventsNamespace = ".IntegrationEvents";

    public async Task Before(Envelope envelope)
    {
        if (envelope.Message is null)
            return;

        var messageType = envelope.Message.GetType();

        if (!IsIntegrationEvent(messageType))
            return;

        var cloudEvent = new CloudEvent
        {
            Id = Guid.CreateVersion7().ToString(),
            Type = messageType.Name,
            Source = serviceBusOptions.Value.ServiceUrn,
            Time = DateTimeOffset.UtcNow,
            Data = envelope.Message,
            DataContentType = "application/json"
        };

        envelope.Message = cloudEvent;
        envelope.ContentType = "application/cloudevents+json";

        await Task.CompletedTask;
    }

    private static bool IsIntegrationEvent(Type messageType) => messageType.Namespace?.EndsWith(IntegrationEventsNamespace) == true;
}
