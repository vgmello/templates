# Dapper Extensions

This guide covers the Dapper extensions provided by the Operations platform for enhanced database operations.

## Overview

The Operations platform provides comprehensive Dapper extensions through the `Operations.Extensions.Dapper` and `Operations.Extensions.Abstractions.Dapper` packages, enabling strongly-typed database operations with source generation.

## Core Extensions

### DbDataSourceExtensions

The `DbDataSourceExtensions` class provides extension methods for `DbDataSource` to simplify database operations:

```csharp
public static class DbDataSourceExtensions
{
    public static async Task<T?> QuerySingleOrDefaultAsync<T>(
        this DbDataSource dataSource,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        using var connection = await dataSource.OpenConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<T>(sql, param, transaction, commandTimeout, commandType);
    }

    public static async Task<IEnumerable<T>> QueryAsync<T>(
        this DbDataSource dataSource,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        using var connection = await dataSource.OpenConnectionAsync();
        return await connection.QueryAsync<T>(sql, param, transaction, commandTimeout, commandType);
    }

    public static async Task<int> ExecuteAsync(
        this DbDataSource dataSource,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        using var connection = await dataSource.OpenConnectionAsync();
        return await connection.ExecuteAsync(sql, param, transaction, commandTimeout, commandType);
    }
}
```

### Usage Examples

```csharp
public class CashierRepository
{
    private readonly DbDataSource _dataSource;

    public CashierRepository(DbDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<Cashier?> GetCashierAsync(Guid id)
    {
        const string sql = "SELECT * FROM billing.cashiers WHERE id = @Id";
        return await _dataSource.QuerySingleOrDefaultAsync<Cashier>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Cashier>> GetActiveCashiersAsync()
    {
        const string sql = "SELECT * FROM billing.cashiers WHERE is_active = true";
        return await _dataSource.QueryAsync<Cashier>(sql);
    }

    public async Task<int> CreateCashierAsync(Cashier cashier)
    {
        const string sql = """
            INSERT INTO billing.cashiers (id, name, email, currencies, is_active, created_at, updated_at)
            VALUES (@Id, @Name, @Email, @Currencies, @IsActive, @CreatedAt, @UpdatedAt)
            """;
        
        return await _dataSource.ExecuteAsync(sql, cashier);
    }
}
```

## Source Generation Support

### DbCommand Attribute

The `DbCommandAttribute` enables compile-time source generation for database operations:

```csharp
[DbCommand("SELECT * FROM billing.cashiers WHERE id = @Id")]
public partial Task<Cashier?> GetCashierAsync(Guid id);

[DbCommand("INSERT INTO billing.cashiers (id, name, email, currencies, is_active, created_at, updated_at) VALUES (@Id, @Name, @Email, @Currencies, @IsActive, @CreatedAt, @UpdatedAt)")]
public partial Task<int> CreateCashierAsync(Cashier cashier);

[DbCommand("UPDATE billing.cashiers SET name = @Name, email = @Email, currencies = @Currencies, updated_at = @UpdatedAt WHERE id = @Id")]
public partial Task<int> UpdateCashierAsync(Cashier cashier);
```

### Generated Code Example

The source generator creates implementations like:

```csharp
// Generated code
public partial Task<Cashier?> GetCashierAsync(Guid id)
{
    const string sql = "SELECT * FROM billing.cashiers WHERE id = @Id";
    return _dataSource.QuerySingleOrDefaultAsync<Cashier>(sql, new { Id = id });
}

public partial Task<int> CreateCashierAsync(Cashier cashier)
{
    const string sql = "INSERT INTO billing.cashiers (id, name, email, currencies, is_active, created_at, updated_at) VALUES (@Id, @Name, @Email, @Currencies, @IsActive, @CreatedAt, @UpdatedAt)";
    return _dataSource.ExecuteAsync(sql, cashier);
}
```

## Advanced Features

### Column Mapping

Use the `ColumnAttribute` to map properties to database column names:

```csharp
public class Cashier
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    [Column("currencies")]
    public List<string> SupportedCurrencies { get; set; } = [];
    
    [Column("is_active")]
    public bool IsActive { get; set; }
    
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
    
    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }
}
```

### Custom Parameter Providers

Implement `IDbParamsProvider` for custom parameter handling:

```csharp
public class CashierParamsProvider : IDbParamsProvider<Cashier>
{
    public object GetParams(Cashier item, DbParamsCase paramsCase = DbParamsCase.Default)
    {
        return paramsCase switch
        {
            DbParamsCase.Insert => new
            {
                item.Id,
                item.Name,
                item.Email,
                Currencies = item.SupportedCurrencies.ToArray(),
                item.IsActive,
                item.CreatedAt,
                item.UpdatedAt
            },
            DbParamsCase.Update => new
            {
                item.Name,
                item.Email,
                Currencies = item.SupportedCurrencies.ToArray(),
                item.UpdatedAt,
                item.Id // WHERE condition
            },
            _ => item
        };
    }
}
```

### Repository Pattern with Extensions

```csharp
public class CashierRepository : ICashierRepository
{
    private readonly DbDataSource _dataSource;

    public CashierRepository(DbDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    [DbCommand("SELECT * FROM billing.cashiers WHERE id = @Id")]
    public partial Task<Cashier?> GetByIdAsync(Guid id);

    [DbCommand("SELECT * FROM billing.cashiers WHERE email = @Email")]
    public partial Task<Cashier?> GetByEmailAsync(string email);

    [DbCommand("SELECT * FROM billing.cashiers WHERE is_active = true ORDER BY name")]
    public partial Task<IEnumerable<Cashier>> GetActiveAsync();

    [DbCommand("SELECT COUNT(*) FROM billing.cashiers WHERE is_active = true")]
    public partial Task<int> GetActiveCountAsync();

    [DbCommand("CALL billing.cashier_create(@Id, @Name, @Email, @Currencies, @IsActive, @CreatedAt, @UpdatedAt)")]
    public partial Task CreateAsync(Cashier cashier);

    [DbCommand("CALL billing.cashier_update(@Id, @Name, @Email, @Currencies, @UpdatedAt)")]
    public partial Task UpdateAsync(Cashier cashier);

    [DbCommand("CALL billing.cashier_delete(@Id)")]
    public partial Task DeleteAsync(Guid id);

    public async Task<(IEnumerable<Cashier> Cashiers, int TotalCount)> GetPagedAsync(
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        const string countSql = "SELECT COUNT(*) FROM billing.cashiers";
        const string dataSql = """
            SELECT * FROM billing.cashiers 
            ORDER BY name 
            LIMIT @PageSize OFFSET @Offset
            """;

        var offset = (page - 1) * pageSize;
        var parameters = new { PageSize = pageSize, Offset = offset };

        var totalCount = await _dataSource.QuerySingleAsync<int>(countSql, cancellationToken: cancellationToken);
        var cashiers = await _dataSource.QueryAsync<Cashier>(dataSql, parameters, cancellationToken: cancellationToken);

        return (cashiers, totalCount);
    }
}
```

## Transaction Support

### Using Transactions

```csharp
public class BillingService
{
    private readonly DbDataSource _dataSource;

    public BillingService(DbDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<Invoice> CreateInvoiceWithCashierAsync(
        CreateInvoiceCommand command, 
        CancellationToken cancellationToken = default)
    {
        using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            // Verify cashier exists
            var cashier = await connection.QuerySingleOrDefaultAsync<Cashier>(
                "SELECT * FROM billing.cashiers WHERE id = @Id AND is_active = true",
                new { Id = command.CashierId },
                transaction);

            if (cashier == null)
            {
                throw new InvalidOperationException("Cashier not found or inactive");
            }

            // Create invoice
            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                CashierId = command.CashierId,
                Amount = command.Amount,
                Currency = command.Currency,
                Status = InvoiceStatus.Draft,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await connection.ExecuteAsync(
                "CALL billing.invoice_create(@Id, @CashierId, @Amount, @Currency, @Status, @CreatedAt, @UpdatedAt)",
                invoice,
                transaction);

            // Update cashier statistics
            await connection.ExecuteAsync(
                "UPDATE billing.cashiers SET total_invoices = total_invoices + 1, updated_at = @UpdatedAt WHERE id = @Id",
                new { Id = command.CashierId, UpdatedAt = DateTimeOffset.UtcNow },
                transaction);

            await transaction.CommitAsync(cancellationToken);
            return invoice;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
```

## Configuration and Setup

### Service Registration

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBillingDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register PostgreSQL data source
        services.AddNpgsqlDataSource(
            configuration.GetConnectionString("Billing")!,
            builder =>
            {
                builder.EnableParameterLogging();
                builder.EnableSensitiveDataLogging();
            });

        // Register repositories
        services.AddScoped<ICashierRepository, CashierRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();

        return services;
    }
}
```

### Connection String Configuration

```json
{
  "ConnectionStrings": {
    "Billing": "Host=localhost;Database=billing;Username=postgres;Password=password;Include Error Detail=true;Log Parameters=true"
  }
}
```

## Best Practices

1. **Connection Management**: Always use `using` statements or let the extensions handle connection lifecycle
2. **Parameter Binding**: Use parameterized queries to prevent SQL injection
3. **Transaction Scope**: Keep transactions as short as possible
4. **Error Handling**: Implement proper exception handling for database operations
5. **Cancellation Tokens**: Always pass cancellation tokens for async operations
6. **Source Generation**: Use `DbCommand` attributes for compile-time safety
7. **Column Mapping**: Use `Column` attributes for explicit mapping when needed

## Performance Considerations

### Connection Pooling

```csharp
// Configure connection pooling
services.AddNpgsqlDataSource(connectionString, builder =>
{
    builder.ConnectionString = connectionString;
    builder.MaxPoolSize = 100;
    builder.MinPoolSize = 10;
    builder.ConnectionIdleLifetime = TimeSpan.FromMinutes(15);
    builder.ConnectionPruningInterval = TimeSpan.FromMinutes(10);
});
```

### Query Optimization

```csharp
// Use specific columns instead of SELECT *
[DbCommand("SELECT id, name, email, is_active FROM billing.cashiers WHERE id = @Id")]
public partial Task<CashierSummary?> GetCashierSummaryAsync(Guid id);

// Use appropriate indexes
[DbCommand("SELECT * FROM billing.cashiers WHERE email = @Email")] // Ensure index on email
public partial Task<Cashier?> GetByEmailAsync(string email);
```

## See Also

- [Database Integration Overview](overview.md)
- [Source Generators](source-generators.md)
- [Service Defaults](../architecture/service-defaults.md)