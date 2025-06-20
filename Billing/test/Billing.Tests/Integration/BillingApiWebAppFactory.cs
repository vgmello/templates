// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.AspNetCore.Mvc.Testing;
using Operations.ServiceDefaults.Api;
using Operations.ServiceDefaults.Messaging.Wolverine;
using Testcontainers.PostgreSql;
using DotNet.Testcontainers.Networks;
using DotNet.Testcontainers.Builders;

namespace Billing.Tests.Integration;

public class BillingApiWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly INetwork _network = new NetworkBuilder().Build();

    private readonly PostgreSqlContainer _postgres;

    public BillingApiWebAppFactory()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithNetwork(_network)
            .Build();
    }

    public async ValueTask InitializeAsync()
    {
        await _network.CreateAsync();
        await _postgres.StartAsync();
        await RunLiquibaseMigrations(_postgres.Name, _network);
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
                ["ConnectionString:ServiceBus"] = _postgres.GetConnectionString()
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

    private static async Task RunLiquibaseMigrations(string dbServer, INetwork containerNetwork)
    {
        dbServer = dbServer.Trim('/');
        var baseDirectory = Path.GetFullPath("../../../../../");

        var liquibaseContainer = new ContainerBuilder()
            .WithImage("liquibase/liquibase:latest")
            .WithNetwork(containerNetwork)
            .WithBindMount($"{baseDirectory}infra/Billing.Database/Liquibase", "/liquibase/changelog")
            .WithEnvironment("LIQUIBASE_COMMAND_USERNAME", "postgres")
            .WithEnvironment("LIQUIBASE_COMMAND_PASSWORD", "postgres")
            .WithEnvironment("LIQUIBASE_COMMAND_CHANGELOG_FILE", "changelog.xml")
            .WithEnvironment("LIQUIBASE_SEARCH_PATH", "/liquibase/changelog")
            .WithEntrypoint("/bin/sh")
            .WithCommand("-c", $"""
                                liquibase --url=jdbc:postgresql://{dbServer}:5432/postgres update --contexts @setup && \
                                liquibase --url=jdbc:postgresql://{dbServer}:5432/service_bus update --changelog-file=service_bus/changelog.xml && \
                                liquibase --url=jdbc:postgresql://{dbServer}:5432/billing update --changelog-file=billing/changelog.xml && \
                                echo Migration Complete
                                """)
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilMessageIsLogged("Migration Complete", opt => opt.WithTimeout(TimeSpan.FromMinutes(1))))
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
