using Microsoft.Extensions.Diagnostics.HealthChecks;

// #region DatabaseChecks
public class DatabaseHealthCheckService
{
    public void ConfigureHealthChecks(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            // Primary database connection
            .AddNpgSql(
                configuration.GetConnectionString("DefaultConnection")!,
                name: "database",
                tags: new[] { "ready", "database" })
            
            // Read replica connection
            .AddNpgSql(
                configuration.GetConnectionString("ReadOnlyConnection")!,
                name: "database_readonly",
                tags: new[] { "database", "readonly" })
            
            // Custom database performance check
            .AddCheck<DatabasePerformanceHealthCheck>(
                "database_performance",
                tags: new[] { "database", "performance" });
    }
}

public class DatabasePerformanceHealthCheck : IHealthCheck
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DatabasePerformanceHealthCheck(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
            
            var stopwatch = Stopwatch.StartNew();
            await connection.ExecuteScalarAsync("SELECT 1", cancellationToken);
            stopwatch.Stop();
            
            var responseTime = stopwatch.ElapsedMilliseconds;
            
            if (responseTime > 1000)
            {
                return HealthCheckResult.Unhealthy(
                    $"Database response time too slow: {responseTime}ms");
            }
            
            if (responseTime > 500)
            {
                return HealthCheckResult.Degraded(
                    $"Database response time degraded: {responseTime}ms");
            }
            
            return HealthCheckResult.Healthy(
                $"Database responding in {responseTime}ms");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Database connection failed", ex);
        }
    }
}
// #endregion

// Supporting interfaces
public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken);
}