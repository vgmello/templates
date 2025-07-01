# Architecture Overview

The Billing service follows Domain-Driven Design (DDD) principles and implements a microservices architecture with clear separation of concerns.

## Quick Start

### Using .NET Aspire (Recommended)
```bash
cd Billing/src/Billing.AppHost
dotnet run
```

This automatically:
- Sets up PostgreSQL databases with Liquibase migrations
- Starts all services (UI, API, BackOffice, Orleans)
- Configures service discovery and dependencies
- Provides observability dashboard

Access Points:
- **Aspire Dashboard**: http://localhost:18100
- **Billing Web UI**: http://localhost:8105
- **Billing API**: http://localhost:8101/scalar
- **Orleans Dashboard**: http://localhost:8104/dashboard

### Using Docker Compose
```bash
# Backend services only
docker compose -f Billing/compose.yml --profile api up -d

# Backend + documentation
docker compose -f Billing/compose.yml --profile api --profile docs up -d

# All services
docker compose -f Billing/compose.yml --profile api --profile backoffice --profile aspire up -d
```

## Service Structure

### Core Projects

- **Billing** (Domain Layer)
  - Contains business logic, entities, commands, and queries
  - Implements CQRS pattern using WolverineFx
  - Location: `src/Billing/`

- **Billing.Api** (Application Layer)
  - REST API endpoints and gRPC services
  - Controllers and service implementations
  - Location: `src/Billing.Api/`

- **Billing.BackOffice** (Background Processing)
  - Handles integration events and background jobs
  - Processes cross-service communication
  - Location: `src/Billing.BackOffice/`

- **Billing.BackOffice.Orleans** (Stateful Processing)
  - Orleans grains for stateful invoice processing
  - Actor-model pattern implementation
  - Location: `src/Billing.BackOffice.Orleans/`

- **Billing.Contracts** (Integration)
  - Shared models and integration events
  - Contract definitions for inter-service communication
  - Location: `src/Billing.Contracts/`

- **Billing.AppHost** (Orchestration)
  - .NET Aspire configuration for service discovery
  - Development and deployment orchestration
  - Location: `src/Billing.AppHost/`

## Architectural Patterns

### Domain-Driven Design (DDD)

The service implements DDD with:
- **Entities**: Core domain objects with identity (`Cashier`, `Invoice`)
- **Value Objects**: Immutable objects (`CashierPayment`)
- **Aggregates**: Consistency boundaries around related entities
- **Domain Services**: Business logic that doesn't belong to a single entity

### CQRS (Command Query Responsibility Segregation)

Implemented using WolverineFx:
- **Commands**: State-changing operations (`CreateCashierCommand`)
- **Queries**: Data retrieval operations (`GetCashierQuery`, `GetCashiersQuery`)
- **Handlers**: Process commands and queries with clear separation

### Event-Driven Architecture

- **Integration Events**: Cross-service communication (`CashierCreatedEvent`)
- **Event Handlers**: React to events from other services (`BusinessDayEndedHandler`)
- **Message Bus**: WolverineFx provides reliable messaging infrastructure

### Actor Model (Orleans)

- **Grains**: Stateful processing units (`InvoiceGrain`)
- **Persistent State**: Automatic state management (`InvoiceState`)
- **Virtual Actors**: Handle invoice lifecycle with built-in clustering

## Communication Patterns

### REST APIs
Standard HTTP endpoints for external clients:
```csharp
[ApiController]
[Route("[controller]")]
public class CashiersController : ControllerBase
```

### gRPC Services
High-performance inter-service communication:
```protobuf
service CashiersService {
  rpc GetCashier (GetCashierRequest) returns (Cashier);
  rpc GetCashiers (GetCashiersRequest) returns (GetCashiersResponse);
  rpc CreateCashier (CreateCashierRequest) returns (Cashier);
}
```

### Integration Events
Asynchronous messaging between services:
```csharp
public record CashierCreatedEvent(Guid CashierId, string Name, string Email);
```

## Data Architecture

### Database Structure
- **Primary Database**: `billing` schema for domain data
- **Service Bus Database**: `service_bus` for messaging infrastructure
- **Migration Management**: Liquibase with hierarchical changelogs

### Data Access Patterns
- **Source Generators**: Compile-time generation for stored procedures
- **DbCommand Pattern**: Type-safe database operations
- **Entity Framework**: For complex queries and relationships

## Technology Stack

### Core Technologies
- **.NET 9**: Latest runtime with performance improvements
- **WolverineFx**: CQRS and messaging framework
- **Orleans**: Virtual actor framework for stateful processing
- **PostgreSQL**: Primary database with JSONB support
- **Liquibase**: Database migration and versioning

### Observability
- **OpenTelemetry**: Distributed tracing and metrics
- **Serilog**: Structured logging with rich context
- **Health Checks**: Service and dependency health monitoring

### Development Tools
- **.NET Aspire**: Local development orchestration
- **FluentValidation**: Input validation with automatic registration
- **Protocol Buffers**: Schema-first gRPC communication

## Testing Strategy

### Architecture Tests
Enforce architectural boundaries using NetArchTest:
```csharp
[Fact]
public void Domain_Should_Not_Have_Dependency_On_Infrastructure()
{
    // Test implementation
}
```

### Integration Tests
Full-stack testing with real dependencies:
- **WebApplicationFactory**: In-memory test server
- **Testcontainers**: Isolated database instances
- **Real Message Bus**: End-to-end event flow testing

### Unit Tests
Fast, isolated testing of business logic:
- **Command Handlers**: Business rule validation
- **Query Handlers**: Data transformation logic
- **Orleans Grains**: Stateful behavior testing

## Frontend Architecture

### SvelteKit Application
The billing-ui is a modern web application built with:

#### Technology Stack
- **SvelteKit**: Full-stack framework with SSR and client hydration
- **Svelte 5**: Latest version with runes for reactive state
- **TypeScript**: Full type safety across the application
- **Tailwind CSS**: Utility-first styling approach
- **shadcn-svelte**: Accessible, customizable UI components

#### Project Structure
```
web/billing-ui/
├── src/
│   ├── routes/              # SvelteKit pages and API routes
│   │   ├── cashiers/        # Cashier management UI
│   │   └── invoices/        # Invoice management UI
│   ├── lib/
│   │   ├── api/            # API client and service interfaces
│   │   ├── components/     # Reusable UI components
│   │   ├── server/         # Server-side BFF services
│   │   └── types/          # TypeScript type definitions
│   └── app.html            # Application shell
├── e2e/                    # Playwright E2E tests
└── package.json           # Dependencies and scripts
```

#### Key Features
- **Server-Side Rendering**: Fast initial page loads with SEO benefits
- **Form Actions**: Progressive enhancement with JavaScript-optional forms
- **Type-Safe API Client**: Generated types from backend contracts
- **Currency Components**: Specialized input/display for monetary values
- **Responsive Design**: Mobile-first approach with desktop enhancements

#### Development Commands
```bash
cd Billing/web/billing-ui
pnpm install        # Install dependencies
pnpm dev           # Start dev server (http://localhost:5173)
pnpm build         # Build for production
pnpm preview       # Preview production build
pnpm test:unit     # Run Vitest unit tests
pnpm test:e2e      # Run Playwright E2E tests
pnpm lint          # ESLint + Prettier check
pnpm format        # Auto-format code
```

## Infrastructure Architecture

### Database Infrastructure
Located in `infra/Billing.Database/`:

#### Liquibase Migration Structure
```
Liquibase/
├── changelog.xml           # Root changelog
├── billing/               # Domain-specific migrations
│   ├── changelog.xml      # Billing schema changelog
│   ├── tables/           # Table definitions
│   └── procedures/       # Stored procedures
└── service_bus/          # Messaging infrastructure
```

#### Key Database Objects
- **Tables**: cashiers, cashier_currencies, invoices
- **Stored Procedures**: Type-safe CRUD operations
- **Schemas**: billing (domain), service_bus (messaging)

### Configuration Management
- **appsettings.json**: Base configuration
- **appsettings.Development.json**: Local overrides
- **Environment Variables**: Production secrets
- **Aspire Configuration**: Service discovery and dependencies

## Deployment Architecture

### Containerization
- **Docker**: Multi-stage builds for optimized images
- **Health Checks**: Built-in container health monitoring
- **Configuration**: Environment-specific settings

### Service Discovery
- **.NET Aspire**: Automatic service registration and discovery
- **Configuration**: Centralized service configuration
- **Load Balancing**: Built-in client-side load balancing

### Port Allocations
- **8101**: Billing API (REST)
- **8102**: Billing API (gRPC)
- **8103**: BackOffice Service
- **8104**: Orleans Dashboard
- **8105**: Billing UI
- **8119**: Documentation
- **18100**: Aspire Dashboard
- **54320**: PostgreSQL

This architecture provides a robust, scalable foundation for billing operations while maintaining clear boundaries and testability.