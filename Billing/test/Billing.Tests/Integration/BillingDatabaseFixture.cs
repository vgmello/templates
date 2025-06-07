using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Configurations;
using Testcontainers.PostgreSql;
using Npgsql;
using System.IO;
using System.Threading.Tasks;

namespace Billing.Tests.Integration;

public class BillingDatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;

    public NpgsqlDataSource DataSource { get; private set; } = default!;

    public BillingDatabaseFixture()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("billing")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    public async ValueTask InitializeAsync()
    {
        await _dbContainer.StartAsync();

        await ApplyMigrationsAsync();

        var builder = new NpgsqlDataSourceBuilder(_dbContainer.GetConnectionString());
        DataSource = builder.Build();
    }

    private async Task ApplyMigrationsAsync()
    {
        var root = Path.GetFullPath(Path.Combine("..", "..", "..", "..", "infra", "Billing.Database"));

        var container = new ContainerBuilder()
            .WithImage("liquibase/liquibase")
            .WithBindMount(root, "/liquibase")
            .WithCommand(
                "--url=jdbc:postgresql://" + _dbContainer.Hostname + ":" + _dbContainer.GetMappedPublicPort(PostgreSqlBuilder.PostgreSqlPort) + "/billing",
                "--username=postgres",
                "--password=postgres",
                "--changeLogFile=billing/changelog.xml",
                "update")
            .Build();

        await container.StartAsync();
        await container.StopAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}
