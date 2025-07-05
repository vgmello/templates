// Copyright (c) ABCDEG. All rights reserved.

using CloudNative.CloudEvents;

namespace Operations.ServiceDefaults.Messaging.Kafka;

public class CloudEventsSettings
{
    public ContentMode Mode { get; set; } = ContentMode.Structured;

    public EventFormat EventFormat { get; set; } = EventFormat.Avro;
}

public enum EventFormat
{
    Avro,
    Json
}
