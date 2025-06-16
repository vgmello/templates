// Copyright (c) ABCDEG. All rights reserved.

using Operations.Extensions.Abstractions.Messaging;
using System.Diagnostics;
using Wolverine;

namespace Operations.ServiceDefaults.Messaging.Middlewares;

public static class OpenTelemetryInstrumentationMiddleware
{
    public static Activity? Before(ActivitySource activitySource, Envelope envelope)
    {
        var messageType = GetMessageType(envelope);
        var activity = activitySource.StartActivity(messageType);

        if (activity is null)
            return activity;

        activity.SetTag("message.id", envelope.Id.ToString());

        if (envelope.Message is not null)
        {
            activity.SetTag("message.name", envelope.Message.GetType().Name);

            if (IsCommand(envelope.Message))
            {
                activity.SetTag("operation.type", "command");
            }
            else if (IsQuery(envelope.Message))
            {
                activity.SetTag("operation.type", "query");
            }
        }

        if (!string.IsNullOrEmpty(envelope.Source))
        {
            activity.SetTag("message.source", envelope.Source);
        }

        return activity;
    }

    public static void Finally(Activity? activity, Exception? exception)
    {
        if (activity is null)
            return;

        if (exception is null)
        {
            activity.SetStatus(ActivityStatusCode.Ok);
        }
        else
        {
            activity.SetStatus(ActivityStatusCode.Error, exception.Message);
            activity.SetTag("error.type", exception.GetType().Name);
            activity.SetTag("error.message", exception.Message);
        }

        activity.Stop();
    }

    private static string GetMessageType(Envelope envelope) => envelope.Message?.GetType().Name ?? envelope.MessageType ?? "Unknown";

    private static bool IsCommand(object message) =>
        message.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));

    private static bool IsQuery(object message) =>
        message.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>));
}
