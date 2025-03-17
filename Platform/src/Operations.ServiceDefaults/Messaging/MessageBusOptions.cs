// Copyright (c) ABCDEG. All rights reserved.

using Serilog;
using System.ComponentModel.DataAnnotations;

namespace Operations.ServiceDefaults.Messaging;

public class MessageBusOptions
{
    [Required]
    public string ServiceName { get; init; } = GetServiceName();

    public string? ConnectionString { get; init; }

    public static void Validate(MessageBusOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            Log.Warning("Messaging:ConnectionString is not set. " +
                        "Transactional Inbox/Outbox and Message Persistence features disabled");
        }
    }

    private static string GetServiceName() =>
        Extensions.EntryAssembly.GetName().Name?.Replace('.', '_') ?? string.Empty;
}
