// Copyright (c) ABCDEG. All rights reserved.

using Operations.Extensions.Abstractions.Messaging;
using Operations.ServiceDefaults.Messaging.Wolverine;
using System.Diagnostics;
using Wolverine;

namespace Operations.ServiceDefaults.Messaging.Middlewares;

public static class OpenTelemetryInstrumentationMiddleware
{
    public static Activity? Before(ActivitySource activitySource, Envelope envelope)
    {
        var activityName = envelope.GetMessageName();
        var activity = activitySource.StartActivity(activityName);

        if (activity is null)
            return null;

        activity.SetTag("message.id", envelope.Id.ToString());

        if (envelope.Message is not null)
        {
            activity.SetTag("message.name", envelope.GetMessageName(fullName: true));

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

    public static void Finally(Activity? activity, Envelope envelope)
    {
        if (activity is null)
            return;

        if (envelope.Failure is null)
        {
            activity.SetStatus(ActivityStatusCode.Ok);
        }
        else
        {
            activity.SetStatus(ActivityStatusCode.Error, envelope.Failure.Message);
            activity.SetTag("error.type", envelope.Failure.GetType().Name);
        }

        activity.Stop();
    }

    private static bool IsCommand(object message) =>
        message.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));

    private static bool IsQuery(object message) =>
        message.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>));
}
