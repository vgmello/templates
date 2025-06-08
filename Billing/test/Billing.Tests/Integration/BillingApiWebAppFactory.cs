// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.AspNetCore.Mvc.Testing;
using Operations.ServiceDefaults.Api;
using Operations.ServiceDefaults.Messaging.Wolverine;
using Testcontainers.PostgreSql;
using System.Diagnostics;
using DotNet.Testcontainers.Networks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Billing.Tests.Integration;

public class BillingApiWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly INetwork _network = new NetworkBuilder()
        .WithName($"billing-test-{Guid.NewGuid():N}")
        .Build();
        
    private readonly PostgreSqlContainer _postgres;
    
    public BillingApiWebAppFactory()
    {
        _postgres = new PostgreSqlBuilder()
            .WithDatabase("billing")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithNetwork(_network)
            .WithNetworkAliases("postgres")
            .Build();
    }

    public async ValueTask InitializeAsync()
    {
        await _network.CreateAsync();
        await _postgres.StartAsync();
        await RunLiquibaseMigrations();
    }

    public new async ValueTask DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await _network.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:BillingDb"] = _postgres.GetConnectionString(),
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

    private async Task RunLiquibaseMigrations()
    {
        // Run Liquibase migrations using the same approach as Aspire
        var liquibaseContainer = new ContainerBuilder()
            .WithImage("liquibase/liquibase:latest")
            .WithNetwork(_network)
            .WithBindMount("/workspaces/templates/Billing/infra/Billing.Database/Liquibase", "/liquibase/changelog")
            .WithEnvironment("LIQUIBASE_COMMAND_USERNAME", "postgres")
            .WithEnvironment("LIQUIBASE_COMMAND_PASSWORD", "postgres")
            .WithEnvironment("LIQUIBASE_COMMAND_CHANGELOG_FILE", "changelog.xml")
            .WithEnvironment("LIQUIBASE_SEARCH_PATH", "/liquibase/changelog")
            .WithEntrypoint("/bin/sh")
            .WithCommand("-c", 
                "liquibase --url=jdbc:postgresql://postgres:5432/billing update --changelog-file=billing/changelog.xml")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("echo", "ready"))
            .Build();
            
        await liquibaseContainer.StartAsync();
        var result = await liquibaseContainer.GetExitCodeAsync();
        var logs = await liquibaseContainer.GetLogsAsync();
        await liquibaseContainer.DisposeAsync();
        
        if (result != 0)
        {
            throw new InvalidOperationException($"Liquibase migration failed with exit code {result}. Logs: {logs}");
        }
    }


    private static void RemoveHostedServices(IServiceCollection services)
    {
        var hostedServices = services.Where(d => d.ServiceType == typeof(IHostedService)).ToList();

        foreach (var hostedService in hostedServices)
            services.Remove(hostedService);
    }
}
