// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.AspNetCore.Mvc.Testing;
using Operations.ServiceDefaults.Api;
using Operations.ServiceDefaults.Messaging.Wolverine;
using Testcontainers.PostgreSql;
using System.Diagnostics;

namespace Billing.Tests.Integration;

public class BillingApiWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("billing")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
        await RunLiquibaseMigrations();
    }

    public new async ValueTask DisposeAsync()
    {
        await _postgres.DisposeAsync();
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
        await using var connection = new Npgsql.NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();
        
        // Replicate the Liquibase structure for tests
        var setupSql = @"
            CREATE SCHEMA IF NOT EXISTS billing;
            
            CREATE TABLE IF NOT EXISTS billing.cashiers (
                cashier_id UUID PRIMARY KEY,
                name VARCHAR(100) NOT NULL,
                email VARCHAR(100),
                created_date_utc TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT timezone('utc', now()),
                updated_date_utc TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT timezone('utc', now()),
                version INTEGER NOT NULL DEFAULT 1
            );
            
            CREATE OR REPLACE FUNCTION billing.create_cashier(
                cashier_id uuid,
                name varchar(100),
                email varchar(100)
            )
            RETURNS void
            LANGUAGE plpgsql
            AS $$
            BEGIN
                INSERT INTO billing.cashiers(cashier_id, name, email)
                VALUES (cashier_id, name, email);
            END;
            $$;
        ";
        
        await using var command = new Npgsql.NpgsqlCommand(setupSql, connection);
        await command.ExecuteNonQueryAsync();
    }


    private static void RemoveHostedServices(IServiceCollection services)
    {
        var hostedServices = services.Where(d => d.ServiceType == typeof(IHostedService)).ToList();

        foreach (var hostedService in hostedServices)
            services.Remove(hostedService);
    }
}
