// Copyright (c) ABCDEG. All rights reserved.

using Operations.ServiceDefaults.Api;
using Scalar.AspNetCore;

namespace Housekeeping.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddHousekeepingApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRazorPages();
        services.AddOpenApi();
        services.AddHttpClient();
        services.AddEndpointsApiExplorer();
        services.AddControllers();

        services.AddGrpc(options =>
        {
            options.Interceptors.Add<LoggingGrpcServerInterceptor>();
            options.EnableDetailedErrors = true;
        });

        services.AddScoped(TimeProvider.System);

        return services;
    }

    public static WebApplication UseHousekeepingApi(this WebApplication app)
    {
        // Map health checks
        app.MapDefaultEndpoints();

        app.UseStaticFiles();
        app.UseRouting();

        app.MapRazorPages();
        app.MapControllers();

        // gRPC endpoints
        //app.MapGrpcService<RoomsService>();

        // API Documentation
        app.MapOpenApi();
        app.MapScalarApiReference();

        return app;
    }
}