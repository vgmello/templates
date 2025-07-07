using Microsoft.Extensions.DependencyInjection;
using Operations.ServiceDefaults.Logging;
using Operations.ServiceDefaults.OpenTelemetry;

namespace Operations.ServiceDefaults;

public static class ServiceConfiguration
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddLogging();
        services.AddOpenTelemetry();
        services.AddHealthChecks();
        return services;
    }
}
