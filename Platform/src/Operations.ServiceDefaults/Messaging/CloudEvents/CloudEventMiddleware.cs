using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Wolverine;
using Wolverine.Runtime;

namespace Operations.ServiceDefaults.Messaging.CloudEvents;

public class CloudEventMiddleware
{
    private readonly IConfiguration _configuration;

    public CloudEventMiddleware(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task Before(Envelope envelope)
    {
        if (envelope.Message == null) return;

        var messageType = envelope.Message.GetType();
        
        if (!IsIntegrationEvent(messageType))
            return;

        var serviceName = _configuration["ServiceName"] ?? "Unknown";
        
        var cloudEvent = new CloudEvent
        {
            Id = Guid.NewGuid().ToString(),
            Type = messageType.Name,
            Source = new Uri($"urn:{serviceName.ToLowerInvariant()}"),
            Time = DateTimeOffset.UtcNow,
        };

        cloudEvent.Data = envelope.Message;
        cloudEvent.DataContentType = "application/json";

        envelope.Message = cloudEvent;
        envelope.ContentType = "application/cloudevents+json";
        
        await Task.CompletedTask;
    }

    private static bool IsIntegrationEvent(Type messageType)
    {
        return messageType.Namespace?.EndsWith(".IntegrationEvents") == true;
    }
}