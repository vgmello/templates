// Copyright (c) ABCDEG. All rights reserved.

using System.Diagnostics;
using Operations.Extensions.Messaging;
using Wolverine;

namespace Operations.ServiceDefaults.Messaging.Middlewares;

public static class OpenTelemetryInstrumentationMiddleware
{
    private static readonly ActivitySource ActivitySource = new("Operations.Messaging");

    public static Activity? Before(Envelope envelope)
    {
        var messageType = GetMessageType(envelope);
        var operationName = GetOperationName(envelope, messageType);
        
        var activity = ActivitySource.StartActivity(operationName);
        
        if (activity != null)
        {
            activity.SetTag("message.type", messageType);
            activity.SetTag("message.id", envelope.Id.ToString());
            
            if (envelope.Message != null)
            {
                activity.SetTag("message.name", envelope.Message.GetType().Name);
                
                // Add specific tags for commands and queries
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
        }
        
        return activity;
    }

    public static void Finally(Activity? activity, Exception? exception)
    {
        if (activity != null)
        {
            if (exception != null)
            {
                activity.SetStatus(ActivityStatusCode.Error, exception.Message);
                activity.SetTag("error.type", exception.GetType().Name);
                activity.SetTag("error.message", exception.Message);
            }
            else
            {
                activity.SetStatus(ActivityStatusCode.Ok);
            }
            
            activity.Stop();
        }
    }

    private static string GetMessageType(Envelope envelope) =>
        envelope.Message?.GetType().Name ?? envelope.MessageType ?? "Unknown";

    private static string GetOperationName(Envelope envelope, string messageType)
    {
        if (envelope.Message != null)
        {
            if (IsCommand(envelope.Message))
            {
                return $"command {messageType}";
            }
            
            if (IsQuery(envelope.Message))
            {
                return $"query {messageType}";
            }
        }
        
        return $"message {messageType}";
    }

    private static bool IsCommand(object message)
    {
        var messageType = message.GetType();
        return messageType.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));
    }

    private static bool IsQuery(object message)
    {
        var messageType = message.GetType();
        return messageType.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>));
    }
}