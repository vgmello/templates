// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.AspNetCore.Mvc.Testing;
using Operations.ServiceDefaults.Api;
using Operations.ServiceDefaults.Messaging.Wolverine;

namespace Accounting.Tests.Integration;

public class AccountingApiWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ServiceBus:ConnectionString"] = string.Empty
            });
        });

        WolverineSetupExtensions.SkipServiceRegistration = true;

        builder.ConfigureServices((ctx, services) =>
        {
            RemoveHostedServices(services);

            services.AddWolverineWithDefaults(ctx.Configuration, opt => opt.ApplicationAssembly = typeof(Program).Assembly);
        });

        builder.Configure(app =>
        {
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapGrpcServices(typeof(Program)));
        });
    }

    private static void RemoveHostedServices(IServiceCollection services)
    {
        var hostedServices = services.Where(d => d.ServiceType == typeof(IHostedService)).ToList();

        foreach (var hostedService in hostedServices)
            services.Remove(hostedService);
    }
}
