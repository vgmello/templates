using Microsoft.Extensions.DependencyInjection;

namespace Operations.ServiceDefaults.Api;

public static class ApiConfiguration
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddOpenApiDocument();
        services.AddGrpc();
        services.AddAuthentication();
        services.AddAuthorization();
        return services;
    }
}
