// Copyright (c) ABCDEG. All rights reserved.

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
        await using var connection = new NpgsqlConnection(_dbContainer.GetConnectionString());
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

        await using var command = new NpgsqlCommand(setupSql, connection);
        await command.ExecuteNonQueryAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}
