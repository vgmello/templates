// Copyright (c) ABCDEG. All rights reserved.

using System;
using CloudNative.CloudEvents;
using Microsoft.Extensions.Configuration;
using Wolverine.Runtime;
using System.Text.Json; // Not strictly needed here if SDK handles all JSON work

namespace Operations.ServiceDefaults.Messaging.Wolverine;

/// <summary>
/// A Wolverine envelope policy that wraps outgoing messages identified as
/// integration events into CloudEvents.
/// </summary>
public class CloudEventWrappingPolicy : IEnvelopePolicy
{
    private readonly IConfiguration _configuration;

    public CloudEventWrappingPolicy(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Apply(Envelope envelope, IWolverineRuntime runtime)
    {
        if (envelope.Message == null)
        {
            return;
        }

        var messageType = envelope.Message.GetType();
        bool isIntegrationEvent = messageType.Namespace?.EndsWith(".IntegrationEvents") ?? false;

        if (isIntegrationEvent)
        {
            var serviceName = _configuration["ServiceName"];
            if (string.IsNullOrEmpty(serviceName))
            {
                serviceName = "unknown-service";
                // Consider logging: runtime.Logger.LogWarning("ServiceName not configured, CloudEvent source will be 'unknown-service'.");
            }

            var originalMessage = envelope.Message;

            var cloudEvent = new CloudEvent(CloudEventsSpecVersion.V1_0) // Specify spec version
            {
                Id = Guid.NewGuid().ToString(),
                Type = messageType.Name,
                Source = new Uri($"urn:service:{serviceName}", UriKind.Absolute), // Made Absolute for clarity
                Time = DateTimeOffset.UtcNow,
                DataContentType = "application/json",
                Data = originalMessage
            };

            envelope.Message = cloudEvent;
            envelope.ContentType = CloudEventsSpecVersion.V1_0.MediaType;
        }
    }
}
