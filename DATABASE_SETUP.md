# Liquibase Database

This document explains how to set up the databases for the Operations project using Liquibase. The project uses PostgreSQL as the database engine and Liquibase for database schema management and versioning.

## Setup Instructions

Navigate to the Billing Database project directory:

```bash
cd Billing/infra/Billing.Database/
```

### Step 1: Database Creation Setup

Run the setup command to create the initial database infrastructure:

```bash
liquibase update --defaults-file liquibase.setup.properties
```

**What this does:**

-   Connects to the default PostgreSQL `postgres` database
-   Executes changesets with the `@setup` context
-   Creates necessary databases and initial infrastructure
-   Sets up user permissions and basic configuration

### Step 2: Service Bus Database Setup

Create the service bus database schema:

```bash
liquibase update --defaults-file liquibase.servicebus.properties
```

**What this does:**

-   Connects to the `service_bus` database
-   Executes all changesets in `service_bus/changelog.xml`
-   Creates tables and schemas for service bus functionality
-   Sets up message queuing and event handling structures

### Step 3: Domain Database Setup

Create the main billing domain database:

```bash
liquibase update
```

**What this does:**

-   Uses the default `liquibase.properties` configuration
-   Connects to the `billing` database
-   Executes all changesets in `billing/changelog.xml`
-   Creates all domain-specific tables, indexes, and constraints
-   Sets up the main application database schema

## CI/CD
You can always override the database connection string by providing it in as a CLI parameter

```bash
liquibase update --url=jdbc:postgresql://my-remote-server:5432/service_bus --defaults-file liquibase.setup.properties
```

## Changelog Structure

The project uses a hierarchical changelog structure:

```
Liquibase/
├── changelog.xml (Master changelog)
├── billing/
│   ├── changelog.xml
│   ├── billing.sql
│   └── tables/
└── service_bus/
    ├── changelog.xml
    └── service_bus.sql
```

### Master Changelog

The root `changelog.xml` uses `includeAll` to automatically include all sub-changelogs:

```xml
<includeAll path="" minDepth="2" relativeToChangelogFile="true" endsWithFilter="changelog.xml"/>
```

This approach allows for modular database schema management across different components.

## Troubleshooting

### Common Issues

1. **Connection refused**: Ensure PostgreSQL is running on `localhost:5432`
2. **Database does not exist**: Run the setup step first to create required databases
3. **Permission denied**: Verify database user has sufficient privileges
4. **Changelog lock**: If interrupted, run `liquibase releaseLocks` to clear locks

### Verification Commands

After each step, you can verify the setup:

```bash
# Check database connections
liquibase status --defaults-file liquibase.setup.properties
liquibase status --defaults-file liquibase.servicebus.properties
liquibase status

# List applied changesets
liquibase history --defaults-file liquibase.setup.properties
liquibase history --defaults-file liquibase.servicebus.properties
liquibase history
```

### Rolling Back Changes

If you need to rollback changes:

```bash
# Rollback last changeset
liquibase rollback-count 1

# Rollback to specific tag
liquibase rollback [tag-name]

# Rollback to specific date
liquibase rollback-to-date [YYYY-MM-DD]
```

## Environment-Specific Configuration

For different environments (development, staging, production), you may need to:

1. **Update connection URLs** in the properties files
2. **Configure credentials** using environment variables or separate credential files
3. **Use different contexts** for environment-specific changesets
4. **Set up SSL connections** for production environments

### Environment Variables

Consider using environment variables for sensitive configuration:

```bash
export LIQUIBASE_COMMAND_USERNAME=your_username
export LIQUIBASE_COMMAND_PASSWORD=your_password
```

## Best Practices

1. **Always run setup first** - The databases must be created before domain schemas
2. **Run in sequence** - Follow the three-step process in order
3. **Backup before changes** - Always backup production databases before running updates
4. **Test in development** - Validate all changes in development environment first
5. **Monitor execution** - Watch for errors and warnings during execution
6. **Version control** - Keep all Liquibase files in version control

## Additional Resources

-   [Liquibase Documentation](https://docs.liquibase.com/)
-   [PostgreSQL Documentation](https://www.postgresql.org/docs/)
-   Project-specific schema documentation (see individual changelog files)

---

**Note**: This setup process creates the complete database infrastructure for the Operations project. Ensure all steps are completed successfully before starting the application services.
