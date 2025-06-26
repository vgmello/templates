// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Operations.ServiceDefaults.Messaging;

public class ServiceBusOptions
{
    public static string SectionName => "ServiceBus";

    public string PublicServiceName { get; set; } = string.Empty;

    public Uri ServiceUrn { get; private set; } = null!;

    public static string GetServiceName(string appName) => appName.ToLowerInvariant().Replace('.', '-');

    public class Configurator(ILogger<Configurator> logger, IHostEnvironment env, IConfiguration config)
        : IPostConfigureOptions<ServiceBusOptions>
    {
        public void PostConfigure(string? name, ServiceBusOptions options)
        {
            if (options.PublicServiceName.Length == 0)
                options.PublicServiceName = GetServiceName(env.ApplicationName);

            options.ServiceUrn = new Uri($"urn:{GetServiceName(options.PublicServiceName)}");

            var connectionString = config.GetConnectionString(SectionName);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                logger.LogWarning("ConnectionStrings:ServiceBus is not set. " +
                                  "Transactional Inbox/Outbox and Message Persistence features disabled");
            }
        }
    }
}
