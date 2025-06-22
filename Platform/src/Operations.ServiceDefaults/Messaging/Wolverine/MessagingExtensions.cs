using Microsoft.Extensions.Hosting;

namespace Operations.ServiceDefaults.Messaging.Wolverine;

public static class MessagingExtensions
{
    public static IHostApplicationBuilder AddMessaging(this IHostApplicationBuilder builder)
    {
        return builder;
    }
}