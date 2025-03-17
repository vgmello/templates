// Copyright (c) ABCDEG. All rights reserved.

using Serilog;
using System.ComponentModel.DataAnnotations;

namespace Operations.ServiceDefaults.Messaging;

public class MessageBusOptions
{
    [Required]
    public string ServiceName { get; init; } = Extensions.EntryAssembly.GetName().Name ?? string.Empty;

    public string? ConnectionString { get; init; }

    public static void Validate(MessageBusOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            Log.Warning("Messaging:ConnectionString is not set. " +
                        "Transactional Inbox/Outbox and Message Persistence features disabled");
        }
    }
}
