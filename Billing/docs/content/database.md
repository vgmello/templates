# Database Schema

The Billing service uses PostgreSQL with a well-defined schema managed through Liquibase migrations.

## Database Architecture

### Database Structure
- **Primary Database**: `billing` - Contains domain-specific tables and procedures
- **Service Bus Database**: `service_bus` - Shared messaging infrastructure
- **Migration Tool**: Liquibase with hierarchical changelogs

### Migration Configuration Files
- `liquibase.properties` - Main domain schema migrations
- `liquibase.servicebus.properties` - Service bus schema migrations  
- `liquibase.setup.properties` - Database setup and initialization

## Core Tables

### billing.cashiers

Primary table for cashier management.

```sql
CREATE TABLE billing.cashiers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    version INTEGER NOT NULL DEFAULT 1
);
```

**Fields:**
- `id`: Unique identifier (UUID, auto-generated)
- `name`: Cashier's full name
- `email`: Unique email address
- `created_at`: Record creation timestamp
- `updated_at`: Last modification timestamp
- `version`: Optimistic concurrency control version

**Indexes:**
- Primary key on `id`
- Unique index on `email`
- Index on `created_at` for time-based queries

### billing.cashier_currencies

Manages multi-currency support for cashiers.

```sql
CREATE TABLE billing.cashier_currencies (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    cashier_id UUID NOT NULL REFERENCES billing.cashiers(id),
    currency_code CHAR(3) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

**Fields:**
- `id`: Unique identifier
- `cashier_id`: Foreign key to cashiers table
- `currency_code`: ISO 4217 currency code (USD, EUR, etc.)
- `is_active`: Whether this currency is currently active
- `created_at`: Record creation timestamp

**Indexes:**
- Primary key on `id`
- Foreign key index on `cashier_id`
- Composite index on `(cashier_id, currency_code)`

### billing.invoices

Stores invoice information and status.

```sql
CREATE TABLE billing.invoices (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    cashier_id UUID REFERENCES billing.cashiers(id),
    amount DECIMAL(19,4) NOT NULL,
    currency_code CHAR(3) NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    metadata JSONB,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    version INTEGER NOT NULL DEFAULT 1
);
```

**Fields:**
- `id`: Unique identifier
- `cashier_id`: Optional foreign key to cashiers table
- `amount`: Invoice amount with 4 decimal precision
- `currency_code`: ISO 4217 currency code
- `status`: Invoice status (Pending, Paid, Cancelled, etc.)
- `metadata`: Flexible JSONB field for additional data
- `created_at`: Record creation timestamp
- `updated_at`: Last modification timestamp
- `version`: Optimistic concurrency control version

**Indexes:**
- Primary key on `id`
- Index on `cashier_id`
- Index on `status` for status-based queries
- Index on `created_at` for time-based queries
- GIN index on `metadata` for JSONB queries

## Stored Procedures

### billing.create_cashier

Atomically creates a cashier with validation and audit logging.

```sql
CREATE OR REPLACE FUNCTION billing.create_cashier(
    p_name VARCHAR(255),
    p_email VARCHAR(255)
)
RETURNS TABLE(
    id UUID,
    name VARCHAR(255),
    email VARCHAR(255),
    created_at TIMESTAMP WITH TIME ZONE,
    updated_at TIMESTAMP WITH TIME ZONE,
    version INTEGER
)
LANGUAGE plpgsql
AS $$
BEGIN
    -- Validation logic
    IF p_name IS NULL OR LENGTH(TRIM(p_name)) = 0 THEN
        RAISE EXCEPTION 'Name cannot be empty';
    END IF;

    IF p_email IS NULL OR LENGTH(TRIM(p_email)) = 0 THEN
        RAISE EXCEPTION 'Email cannot be empty';
    END IF;

    -- Insert and return the new cashier
    RETURN QUERY
    INSERT INTO billing.cashiers (name, email)
    VALUES (TRIM(p_name), LOWER(TRIM(p_email)))
    RETURNING 
        billing.cashiers.id,
        billing.cashiers.name,
        billing.cashiers.email,
        billing.cashiers.created_at,
        billing.cashiers.updated_at,
        billing.cashiers.version;
END;
$$;
```

**Parameters:**
- `p_name`: Cashier name (validated for non-empty)
- `p_email`: Cashier email (validated and normalized to lowercase)

**Returns:**
- Complete cashier record with generated fields

**Features:**
- Input validation and sanitization
- Automatic timestamp generation
- Atomic operation with rollback on error
- Consistent error handling

## Service Bus Schema

### service_bus.outbox

Implements the Outbox pattern for reliable message publishing.

```sql
CREATE TABLE service_bus.outbox (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    message_type VARCHAR(255) NOT NULL,
    payload JSONB NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    processed_at TIMESTAMP WITH TIME ZONE,
    retry_count INTEGER NOT NULL DEFAULT 0,
    max_retries INTEGER NOT NULL DEFAULT 3,
    next_retry_at TIMESTAMP WITH TIME ZONE
);
```

### service_bus.inbox

Implements the Inbox pattern for idempotent message processing.

```sql
CREATE TABLE service_bus.inbox (
    id UUID PRIMARY KEY,
    message_type VARCHAR(255) NOT NULL,
    processed_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    payload JSONB
);
```

## Data Access Patterns

### Source Generator Integration

The service uses source generators for type-safe database operations:

```csharp
[DbCommand("SELECT * FROM billing.cashiers WHERE id = @id")]
public partial class GetCashierQuery
{
    public Guid Id { get; set; }
}
```

This generates:
- Parameter binding methods
- Result mapping logic
- Compile-time validation

### Entity Framework Integration

For complex queries and relationships:

```csharp
public class BillingDbContext : DbContext
{
    public DbSet<Cashier> Cashiers { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cashier>()
            .HasMany(c => c.Invoices)
            .WithOne(i => i.Cashier)
            .HasForeignKey(i => i.CashierId);
    }
}
```

## Migration Management

### Liquibase Changelog Structure

```xml
<?xml version="1.0" encoding="UTF-8"?>
<databaseChangeLog xmlns="http://www.liquibase.org/xml/ns/dbchangelog">
    
    <!-- Include domain-specific changelogs -->
    <include file="billing/changelog.xml" relativeToChangelogFile="true"/>
    <include file="service_bus/changelog.xml" relativeToChangelogFile="true"/>
    
</databaseChangeLog>
```

### Domain Changelog Example

```xml
<databaseChangeLog xmlns="http://www.liquibase.org/xml/ns/dbchangelog">
    
    <changeSet id="001-create-cashiers-table" author="billing-team">
        <sqlFile path="tables/cashiers.sql" relativeToChangelogFile="true"/>
    </changeSet>
    
    <changeSet id="002-create-cashier-currencies-table" author="billing-team">
        <sqlFile path="tables/cashier_currencies.sql" relativeToChangelogFile="true"/>
    </changeSet>
    
    <changeSet id="003-create-cashier-procedure" author="billing-team">
        <sqlFile path="procedures/create_cashier.sql" relativeToChangelogFile="true"/>
    </changeSet>
    
</databaseChangeLog>
```

### Running Migrations

```bash
# Check current database status
liquibase status

# Apply all pending migrations
liquibase update

# Rollback last migration
liquibase rollback-count 1

# View migration history
liquibase history
```

## Performance Considerations

### Indexing Strategy
- Primary keys on all tables for fast lookups
- Foreign key indexes for join performance
- Composite indexes for common query patterns
- GIN indexes for JSONB metadata searches

### Partitioning
For high-volume tables like invoices, consider partitioning by:
- Date ranges (monthly/quarterly partitions)
- Status values (active vs. archived)
- Currency codes for multi-tenant scenarios

### Connection Pooling
- Use connection pooling for optimal performance
- Configure appropriate pool sizes based on load
- Monitor connection usage and adjust as needed

This database schema provides a solid foundation for the Billing service with proper normalization, indexing, and migration management.