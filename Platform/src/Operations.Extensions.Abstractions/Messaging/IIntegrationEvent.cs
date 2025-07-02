// Copyright (c) ABCDEG. All rights reserved.

namespace Operations.Extensions.Abstractions.Messaging;

public interface IIntegrationEvent
{
    string GetPartitionKey();
}
