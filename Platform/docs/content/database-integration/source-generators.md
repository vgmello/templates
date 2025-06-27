# Source Generators for Database Operations

This guide covers the source generators provided by the Operations platform for compile-time database operation generation.

## Overview

The Operations platform includes the `Operations.Extensions.SourceGenerators.DbCommand` package that provides compile-time source generation for database operations, ensuring type safety and eliminating runtime reflection.

## DbCommand Source Generator

### Basic Usage

The `DbCommandSourceGenerator` analyzes methods decorated with the `DbCommandAttribute` and generates the implementation:

```csharp
public partial class CashierRepository
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

    [DbCommand("SELECT * FROM billing.cashiers WHERE is_active = true")]
    public partial Task<IEnumerable<Cashier>> GetActiveAsync();
}
```

### Generated Code

The source generator creates implementations similar to:

```csharp
// Generated code (simplified)
public partial class CashierRepository
{
    public partial async Task<Cashier?> GetByIdAsync(Guid id)
    {
        const string sql = "SELECT * FROM billing.cashiers WHERE id = @Id";
        using var connection = await _dataSource.OpenConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<Cashier>(sql, new { Id = id });
    }

    public partial async Task<Cashier?> GetByEmailAsync(string email)
    {
        const string sql = "SELECT * FROM billing.cashiers WHERE email = @Email";
        using var connection = await _dataSource.OpenConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<Cashier>(sql, new { Email = email });
    }

    public partial async Task<IEnumerable<Cashier>> GetActiveAsync()
    {
        const string sql = "SELECT * FROM billing.cashiers WHERE is_active = true";
        using var connection = await _dataSource.OpenConnectionAsync();
        return await connection.QueryAsync<Cashier>(sql);
    }
}
```

## Supported Method Signatures

### Query Operations

```csharp
// Single result
[DbCommand("SELECT * FROM billing.cashiers WHERE id = @Id")]
public partial Task<Cashier?> GetByIdAsync(Guid id);

// Multiple results
[DbCommand("SELECT * FROM billing.cashiers WHERE is_active = @IsActive")]
public partial Task<IEnumerable<Cashier>> GetByStatusAsync(bool isActive);

// Scalar result
[DbCommand("SELECT COUNT(*) FROM billing.cashiers")]
public partial Task<int> GetCountAsync();

// With multiple parameters
[DbCommand("SELECT * FROM billing.cashiers WHERE name LIKE @Name AND is_active = @IsActive")]
public partial Task<IEnumerable<Cashier>> SearchAsync(string name, bool isActive);
```

### Command Operations

```csharp
// Insert
[DbCommand("INSERT INTO billing.cashiers (id, name, email, currencies, is_active, created_at, updated_at) VALUES (@Id, @Name, @Email, @Currencies, @IsActive, @CreatedAt, @UpdatedAt)")]
public partial Task<int> CreateAsync(Cashier cashier);

// Update
[DbCommand("UPDATE billing.cashiers SET name = @Name, email = @Email, updated_at = @UpdatedAt WHERE id = @Id")]
public partial Task<int> UpdateAsync(Cashier cashier);

// Delete
[DbCommand("DELETE FROM billing.cashiers WHERE id = @Id")]
public partial Task<int> DeleteAsync(Guid id);

// Stored procedure
[DbCommand("CALL billing.cashier_create(@Id, @Name, @Email, @Currencies, @IsActive, @CreatedAt, @UpdatedAt)")]
public partial Task CallCreateProcedureAsync(Cashier cashier);
```

## Advanced Features

### Custom Parameter Handling

Use `IDbParamsProvider` for complex parameter scenarios:

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
                item.Id
            },
            _ => item
        };
    }
}

// Usage with custom provider
[DbCommand("INSERT INTO billing.cashiers (id, name, email, currencies, is_active, created_at, updated_at) VALUES (@Id, @Name, @Email, @Currencies, @IsActive, @CreatedAt, @UpdatedAt)", 
    ParamsProvider = typeof(CashierParamsProvider), 
    ParamsCase = DbParamsCase.Insert)]
public partial Task<int> CreateWithProviderAsync(Cashier cashier);
```

### Column Mapping

Map entity properties to database columns:

```csharp
public class Invoice
{
    public Guid Id { get; set; }
    
    [Column("cashier_id")]
    public Guid CashierId { get; set; }
    
    public decimal Amount { get; set; }
    
    public string Currency { get; set; } = string.Empty;
    
    [Column("invoice_status")]
    public InvoiceStatus Status { get; set; }
    
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
    
    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }
}
```

### Complex Queries

```csharp
[DbCommand("""
    SELECT c.*, 
           COUNT(i.id) as invoice_count,
           COALESCE(SUM(i.amount), 0) as total_amount
    FROM billing.cashiers c
    LEFT JOIN billing.invoices i ON c.id = i.cashier_id
    WHERE c.is_active = @IsActive
    GROUP BY c.id, c.name, c.email, c.currencies, c.is_active, c.created_at, c.updated_at
    ORDER BY c.name
    """)]
public partial Task<IEnumerable<CashierWithStats>> GetCashiersWithStatsAsync(bool isActive);

public class CashierWithStats : Cashier
{
    public int InvoiceCount { get; set; }
    public decimal TotalAmount { get; set; }
}
```

## Error Handling and Validation

### Compile-Time Validation

The source generator performs compile-time validation:

```csharp
// Error: Method must be partial
[DbCommand("SELECT * FROM billing.cashiers")]
public Task<IEnumerable<Cashier>> GetAllAsync(); // Error: Method must be partial

// Error: Method must return Task or Task<T>
[DbCommand("SELECT * FROM billing.cashiers")]
public partial IEnumerable<Cashier> GetAll(); // Error: Must return Task

// Error: SQL parameter mismatch
[DbCommand("SELECT * FROM billing.cashiers WHERE id = @Id")]
public partial Task<Cashier?> GetByIdAsync(string name); // Error: Parameter name mismatch
```

### Runtime Error Handling

```csharp
public partial class CashierRepository
{
    [DbCommand("SELECT * FROM billing.cashiers WHERE id = @Id")]
    public partial Task<Cashier?> GetByIdAsync(Guid id);

    // Wrapper method with error handling
    public async Task<Result<Cashier>> GetCashierSafelyAsync(Guid id)
    {
        try
        {
            var cashier = await GetByIdAsync(id);
            return cashier != null 
                ? Result.Success(cashier)
                : Result.Failure<Cashier>("Cashier not found");
        }
        catch (Exception ex)
        {
            return Result.Failure<Cashier>(ex.Message);
        }
    }
}
```

## Performance Considerations

### Connection Management

The generated code automatically handles connection management:

```csharp
// Generated code includes proper connection handling
public partial async Task<Cashier?> GetByIdAsync(Guid id)
{
    const string sql = "SELECT * FROM billing.cashiers WHERE id = @Id";
    using var connection = await _dataSource.OpenConnectionAsync();
    return await connection.QuerySingleOrDefaultAsync<Cashier>(sql, new { Id = id });
}
```

### Query Optimization

Use specific column selections for better performance:

```csharp
// Instead of SELECT *
[DbCommand("SELECT id, name, email FROM billing.cashiers WHERE is_active = true")]
public partial Task<IEnumerable<CashierSummary>> GetActiveSummariesAsync();

public class CashierSummary
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
```

### Batch Operations

```csharp
[DbCommand("""
    INSERT INTO billing.cashiers (id, name, email, currencies, is_active, created_at, updated_at)
    SELECT * FROM UNNEST(
        @Ids::uuid[],
        @Names::text[],
        @Emails::text[],
        @Currencies::text[][],
        @IsActiveFlags::boolean[],
        @CreatedAts::timestamptz[],
        @UpdatedAts::timestamptz[]
    )
    """)]
public partial Task<int> CreateBatchAsync(
    Guid[] ids,
    string[] names,
    string[] emails,
    string[][] currencies,
    bool[] isActiveFlags,
    DateTimeOffset[] createdAts,
    DateTimeOffset[] updatedAts);
```

## Configuration and Setup

### Project Configuration

Add the source generator package to your project:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="Operations.Extensions.SourceGenerators.DbCommand" />
    <PackageReference Include="Operations.Extensions.Dapper" />
    <PackageReference Include="Operations.Extensions.Abstractions.Dapper" />
  </ItemGroup>
</Project>
```

### Service Registration

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGeneratedRepositories(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register data source
        services.AddNpgsqlDataSource(configuration.GetConnectionString("Database")!);

        // Register repositories (generated code will be available)
        services.AddScoped<CashierRepository>();
        services.AddScoped<InvoiceRepository>();
        services.AddScoped<PaymentRepository>();

        return services;
    }
}
```

## Best Practices

1. **Partial Classes**: Always use `partial` classes and methods
2. **Async Operations**: Always return `Task` or `Task<T>` for database operations
3. **Parameter Naming**: Ensure SQL parameter names match method parameter names
4. **SQL Formatting**: Use raw string literals (`"""`) for complex queries
5. **Type Safety**: Leverage compile-time validation to catch errors early
6. **Connection Management**: Let the generator handle connection lifecycle
7. **Error Handling**: Implement proper exception handling around generated methods

## Debugging and Troubleshooting

### Source Generator Debugging

Enable source generator debugging in your project:

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

### Common Issues

1. **Method Not Generated**: Ensure method is `partial` and returns `Task`
2. **Parameter Mismatch**: Check SQL parameter names match method parameters
3. **Type Mapping Issues**: Verify entity properties map to database columns
4. **Build Errors**: Clean and rebuild solution after changes

### Generated Code Inspection

View generated code in the output directory:

```
bin/Debug/net9.0/Generated/Operations.Extensions.SourceGenerators.DbCommand/Operations.Extensions.SourceGenerators.DbCommand.DbCommandSourceGenerator/
```

## See Also

- [Database Integration Overview](overview.md)
- [Dapper Extensions](dapper-extensions.md)
- [Source Generators Overview](../source-generators/overview.md)