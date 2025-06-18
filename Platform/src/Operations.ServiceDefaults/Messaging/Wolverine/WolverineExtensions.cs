// Copyright (c) ABCDEG. All rights reserved.

using Wolverine;

namespace Operations.ServiceDefaults.Messaging.Wolverine;

public static class WolverineExtensions
{
    public static string GetMessageName(this Envelope envelope, bool fullName = false)
    {
        if (envelope.Message?.GetType() is { } messageType)
        {
            if (fullName)
            {
                return messageType.FullName ?? messageType.Name;
            }

            return messageType.Name;
        }

        return envelope.MessageType ?? "UnknownMessage";
    }
}
