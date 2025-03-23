// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace Operations.ServiceDefaults.Messaging;

public class ServiceBusOptions
{
    public static string SectionName => "ServiceBus";

    [Required]
    public string ServiceName { get; init; } = GetServiceName();

    public string? ConnectionString { get; init; }

    private static string GetServiceName() =>
        Extensions.EntryAssembly.GetName().Name?.Replace('.', '_') ?? string.Empty;

    public class Configurator(ILogger<Configurator> logger) : IPostConfigureOptions<ServiceBusOptions>
    {
        public void PostConfigure(string? name, ServiceBusOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                logger.LogWarning("Messaging:ConnectionString is not set. " +
                                  "Transactional Inbox/Outbox and Message Persistence features disabled");
            }
        }
    }
}
