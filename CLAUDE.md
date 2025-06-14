# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Common Commands

### Build and Run
```bash
# Build entire solution
dotnet build Operations.slnx

# Run specific service
dotnet run --project Billing/src/Billing.Api
dotnet run --project Accounting/src/Accounting.Api

# Run tests
dotnet test

# Run with Docker Compose
docker-compose up --build
```

### Database Setup (Required before first run)
```bash
cd Billing/infra/Billing.Database/

# Step 1: Setup databases
liquibase update --defaults-file liquibase.setup.properties

# Step 2: Service bus schema  
liquibase update --defaults-file liquibase.servicebus.properties

# Step 3: Domain schema
liquibase update
```

### Database Management
```bash
# Check migration status
liquibase status

# View migration history
liquibase history

# Rollback last migration
liquibase rollback-count 1
```

## Architecture Overview

This is a .NET 9 microservices system using Domain-Driven Design with these core patterns:

### Service Structure
Each service follows this pattern:
- `*.Api` - REST/gRPC endpoints
- `*.BackOffice` - Background jobs and event handlers  
- `*.Contracts` - Integration events and shared models
- `*` (Core) - Domain logic, commands, queries, entities
- `*.AppHost` - .NET Aspire orchestration
- `*.Tests` - Integration and architecture tests

### Technology Stack
- **Messaging**: WolverineFx for CQRS/Event Sourcing
- **Communication**: gRPC with Protocol Buffers
- **Database**: PostgreSQL with Liquibase migrations
- **Orchestration**: .NET Aspire for service discovery
- **Observability**: OpenTelemetry, Serilog, Health Checks
- **Validation**: FluentValidation with automatic registration

### Key Services
- **Accounting** - Ledgers, business day operations
- **Billing** - Cashiers, invoices, payments
- **Platform/Operations** - Shared infrastructure

## Database Architecture

Multi-database approach with separate databases per service:
- Main domain databases (e.g., `billing`)
- Shared `service_bus` database for messaging
- Hierarchical Liquibase changelogs with automatic inclusion

## Testing Approach

- **xUnit v3** with Microsoft Testing Platform
- **Integration tests** using WebApplicationFactory and Testcontainers
- **Architecture tests** with NetArchTest to enforce layering
- **Mocking** with NSubstitute, assertions with Shouldly

## Development Notes

- **Prerequisites**: PostgreSQL running on localhost:5432
- **Service Discovery**: Automatic via .NET Aspire
- **Package Management**: Centralized via Directory.Packages.props
- **Code Quality**: SonarAnalyzer enabled, warnings as info (not errors)
- **Environment**: .NET 9 with nullable reference types and implicit usings