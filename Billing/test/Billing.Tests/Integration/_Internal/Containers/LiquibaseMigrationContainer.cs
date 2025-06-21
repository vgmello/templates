// Copyright (c) ABCDEG. All rights reserved.

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;

namespace Billing.Tests.Integration._Internal.Containers;

public class LiquibaseMigrationContainer : IAsyncDisposable
{
    private readonly IContainer _liquibaseContainer;

    public LiquibaseMigrationContainer(string dbContainerName, INetwork containerNetwork)
    {
        var dbServerSanitized = dbContainerName.Trim('/');
        var baseDirectory = Path.GetFullPath("../../../../../");

        _liquibaseContainer = new ContainerBuilder()
            .WithImage("liquibase/liquibase:latest")
            .WithNetwork(containerNetwork)
            .WithBindMount($"{baseDirectory}infra/Billing.Database/Liquibase", "/liquibase/changelog")
            .WithEnvironment("LIQUIBASE_COMMAND_USERNAME", "postgres")
            .WithEnvironment("LIQUIBASE_COMMAND_PASSWORD", "postgres")
            .WithEnvironment("LIQUIBASE_COMMAND_CHANGELOG_FILE", "changelog.xml")
            .WithEnvironment("LIQUIBASE_SEARCH_PATH", "/liquibase/changelog")
            .WithEntrypoint("/bin/sh")
            .WithCommand("-c", $"""
                                liquibase --url=jdbc:postgresql://{dbServerSanitized}:5432/postgres update --contexts @setup && \
                                liquibase --url=jdbc:postgresql://{dbServerSanitized}:5432/service_bus update --changelog-file=service_bus/changelog.xml && \
                                liquibase --url=jdbc:postgresql://{dbServerSanitized}:5432/billing update --changelog-file=billing/changelog.xml && \
                                echo Migration Complete
                                """)
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilMessageIsLogged("Migration Complete", opt => opt.WithTimeout(TimeSpan.FromMinutes(1))))
            .Build();
    }

    public async Task StartAsync()
    {
        await _liquibaseContainer.StartAsync();
        var result = await _liquibaseContainer.GetExitCodeAsync();

        if (result != 0)
        {
            var logs = await _liquibaseContainer.GetLogsAsync();

            throw new InvalidOperationException($"Liquibase migration failed with exit code {result}. Logs: {logs}");
        }
    }

    public async ValueTask DisposeAsync() => await _liquibaseContainer.DisposeAsync();
}
