using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Data.Common;

namespace Billing.Examples;

// <keyed_services>
public static class DatabaseServiceRegistration
{
    public static IServiceCollection AddDatabaseServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Default DbDataSource
        services.AddSingleton<DbDataSource>(provider =>
            new NpgsqlDataSourceBuilder(
                configuration.GetConnectionString("DefaultConnection")!)
                .Build());

        // Keyed DbDataSource for reporting (read-only)
        services.AddKeyedSingleton<DbDataSource>("ReportingReader",
            (provider, key) =>
                new NpgsqlDataSourceBuilder(
                    configuration.GetConnectionString("ReportingConnection")!)
                    .Build());

        // Keyed DbDataSource for audit logging
        services.AddKeyedSingleton<DbDataSource>("AuditWriter",
            (provider, key) =>
                new NpgsqlDataSourceBuilder(
                    configuration.GetConnectionString("AuditConnection")!)
                    .Build());

        return services;
    }
}
// </keyed_services>