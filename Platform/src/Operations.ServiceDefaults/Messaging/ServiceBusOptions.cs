// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace Operations.ServiceDefaults.Messaging;

public class ServiceBusOptions
{
    public static string SectionName => "ServiceBus";

    [Required]
    public string ServiceName { get; init; } = GetServiceName();

    private static string GetServiceName() => Extensions.EntryAssembly.GetName().Name?.Replace('.', '_') ?? string.Empty;

    public class Configurator(ILogger<Configurator> logger, IConfiguration config) : IPostConfigureOptions<ServiceBusOptions>
    {
        public void PostConfigure(string? name, ServiceBusOptions options)
        {
            var connectionString = config.GetConnectionString(SectionName);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                logger.LogWarning("ConnectionStrings:ServiceBus is not set. " +
                                  "Transactional Inbox/Outbox and Message Persistence features disabled");
            }
        }
    }
}
