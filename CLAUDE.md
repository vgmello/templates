# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Essential Commands

### Building and Running
- **Start the complete application stack**: `dotnet run --project src/Billing.AppHost`
- **Build all projects**: `dotnet build`
- **Run specific services with Docker**: `docker compose --profile api up` or `docker compose --profile backoffice up`
- **Run database migrations**: `docker compose up billing-db-migrations`

### Testing
- **Run all tests**: `dotnet test`
- **Run specific test project**: `dotnet test tests/Billing.Tests`
- **Run tests with coverage**: `dotnet test --collect:"XPlat Code Coverage"`

### Documentation
- **Start documentation server**: `cd docs && pnpm dev`
- **Build documentation**: `cd docs && pnpm docs:build`
- **Generate event documentation**: `cd docs && pnpm docs:events`

### Database Management
- **Reset database**: `docker compose down -v && docker compose up billing-db billing-db-migrations`
- **Access database**: Connect to `localhost:54320` with credentials `postgres/password@`

## Architecture Overview

This is a .NET 9 microservices solution that follows Domain-Driven Design principles with event-driven architecture. The codebase deliberately mirrors real-world billing department operations to maintain intuitive understanding.

### Core Design Philosophy
- **Real-world mirroring**: Each code component corresponds directly to real billing department roles/operations
- **No smart domain objects**: Entities are data records, not self-modifying objects
- **Front office vs Back office**: Synchronous APIs (front office) vs asynchronous event processing (back office)
- **Minimal abstractions**: Infrastructure elements support functionality like utilities in an office

### Service Structure
```
src/
├── Billing/                    # Core domain logic (Commands, Queries, Events)
├── Billing.Api/               # REST/gRPC endpoints (front office)
├── Billing.AppHost/           # .NET Aspire orchestration
├── Billing.BackOffice/        # Background event processing
├── Billing.BackOffice.Orleans/ # Stateful processing with Orleans
└── Billing.Contracts/         # Integration events and shared models
```

### Key Technologies
- **.NET Aspire**: Application orchestration and service discovery
- **Orleans**: Stateful actor-based processing for invoices
- **Wolverine**: CQRS/MediatR-style command handling with Kafka integration
- **PostgreSQL**: Primary database with Liquibase migrations
- **Apache Kafka**: Event streaming and message bus
- **gRPC + REST**: API protocols
- **Testcontainers**: Integration testing with real infrastructure

### Domain Structure
Each domain area (Cashiers, Invoices) follows consistent patterns:
- `Commands/` - Write operations (CreateCashier, UpdateCashier, etc.)
- `Queries/` - Read operations (GetCashier, GetCashiers, etc.) 
- `Contracts/IntegrationEvents/` - Cross-service events
- `Contracts/Models/` - Shared data contracts
- `Data/` - Database entities and mapping

### Event-Driven Integration
- **Integration Events**: Cross-service communication (CashierCreated, InvoicePaid, etc.)
- **Domain Events**: Internal domain notifications (InvoiceGenerated)
- **Event Documentation**: Auto-generated from XML comments using Operations.Extensions.EventMarkdownGenerator

### Custom Source Generators
The solution includes custom source generators in `libs/Operations/`:
- **DbCommand Generator**: Generates type-safe database command handlers
- **Event Documentation Generator**: Creates markdown documentation from integration events

### Testing Strategy
- **Unit Tests**: Domain logic in `tests/Billing.Tests/Unit/`
- **Integration Tests**: Full stack testing with Testcontainers in `tests/Billing.Tests/Integration/`
- **Architecture Tests**: Enforce architectural constraints using NetArchTest

### Operations Libraries
Shared platform libraries in `libs/Operations/` provide:
- **ServiceDefaults**: Common hosting, logging, OpenTelemetry, health checks
- **Extensions**: CQRS abstractions, database extensions, result types
- **Source Generators**: Code generation for DbCommands and event documentation

### Development Workflow
1. Use .NET Aspire AppHost for local development (starts all services)
2. Database changes go through Liquibase migrations in `infra/Billing.Database/`
3. Integration events require XML documentation for auto-generated docs
4. Follow CQRS patterns - separate command/query handlers
5. Architecture tests enforce design constraints automatically

### Documentation System
- **VitePress**: Documentation framework with TypeScript automation
- **Auto-generated**: Event schemas and API docs generated from code
- **ADR Tracking**: Architecture Decision Records in `docs/arch/adr/`