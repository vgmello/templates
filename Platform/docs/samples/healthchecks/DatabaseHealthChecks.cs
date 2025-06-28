using Microsoft.Extensions.Diagnostics.HealthChecks;
using Operations.ServiceDefaults;
using Operations.ServiceDefaults.Api;
using System.Data;
using System.Data.Common;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApiServiceDefaults();

// <DatabaseChecks>
// Register database health checks with custom configurations
builder.Services.AddHealthChecks()
    // Primary database connectivity check
    .AddNpgSql(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "primary_database",
        tags: new[] { "database", "critical" },
        timeout: TimeSpan.FromSeconds(5))
    
    // Read replica database check
    .AddNpgSql(
        connectionString: builder.Configuration.GetConnectionString("ReadOnlyConnection")!,
        name: "read_replica",
        tags: new[] { "database", "optional" },
        timeout: TimeSpan.FromSeconds(3))
    
    // Custom database performance check
    .AddTypeActivatedCheck<DatabasePerformanceHealthCheck>(
        "database_performance",
        args: new object[] { builder.Configuration.GetConnectionString("DefaultConnection")! },
        tags: new[] { "database", "performance" })
    
    // Connection pool health check
    .AddCheck<ConnectionPoolHealthCheck>(
        "connection_pool",
        tags: new[] { "database", "resources" });
// </DatabaseChecks>

var app = builder.Build();

app.MapDefaultEndpoints();
app.Run();

// <DatabasePerformanceCheck>
public class DatabasePerformanceHealthCheck : IHealthCheck
{
    private readonly string _connectionString;
    private readonly ILogger<DatabasePerformanceHealthCheck> _logger;

    public DatabasePerformanceHealthCheck(string connectionString, ILogger<DatabasePerformanceHealthCheck> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new Npgsql.NpgsqlConnection(_connectionString);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            await connection.OpenAsync(cancellationToken);
            
            // Test a simple query performance
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(cancellationToken);
            
            stopwatch.Stop();
            
            var responseTime = stopwatch.ElapsedMilliseconds;
            var data = new Dictionary<string, object>
            {
                ["response_time_ms"] = responseTime,
                ["connection_state"] = connection.State.ToString()
            };

            if (responseTime > 1000)
            {
                return HealthCheckResult.Degraded($"Database response time is {responseTime}ms", data: data);
            }

            if (responseTime > 500)
            {
                _logger.LogWarning("Database response time is elevated: {ResponseTime}ms", responseTime);
                return HealthCheckResult.Healthy($"Database is healthy but response time is elevated: {responseTime}ms", data);
            }

            return HealthCheckResult.Healthy($"Database is healthy with {responseTime}ms response time", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }
}
// </DatabasePerformanceCheck>

// <ConnectionPoolCheck>
public class ConnectionPoolHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConnectionPoolHealthCheck> _logger;

    public ConnectionPoolHealthCheck(IConfiguration configuration, ILogger<ConnectionPoolHealthCheck> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection")!;
            
            // Simulate checking connection pool metrics
            // In a real implementation, you would check actual pool statistics
            var activeConnections = Random.Shared.Next(1, 10);
            var maxPoolSize = 20;
            var poolUtilization = (double)activeConnections / maxPoolSize * 100;

            var data = new Dictionary<string, object>
            {
                ["active_connections"] = activeConnections,
                ["max_pool_size"] = maxPoolSize,
                ["pool_utilization_percent"] = Math.Round(poolUtilization, 2)
            };

            if (poolUtilization > 90)
            {
                return HealthCheckResult.Degraded($"Connection pool utilization is high: {poolUtilization:F1}%", data: data);
            }

            return HealthCheckResult.Healthy($"Connection pool is healthy: {poolUtilization:F1}% utilization", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Connection pool health check failed");
            return HealthCheckResult.Unhealthy("Connection pool check failed", ex);
        }
    }
}
// </ConnectionPoolCheck>