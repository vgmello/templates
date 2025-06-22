# Accounting Service

The Accounting Service manages ledgers, business day operations, and financial accounting within the broader Operations platform. It provides both REST and gRPC APIs for managing accounting operations and integrates with other services through event-driven messaging.

## What is the Accounting Service?

The Accounting Service is part of a .NET 9 microservices system built using Domain-Driven Design principles. It handles:

- **Ledger Management**: Create and manage ledgers with multi-currency support
- **Business Day Operations**: Handle business day lifecycle with Orleans-based stateful processing  
- **Balance Tracking**: Process balance updates and emit integration events
- **Cross-Service Integration**: React to business events from other services like Billing

## Service Architecture

The Accounting service follows the standard microservices structure with clean separation of concerns:

```
Accounting/
├── src/
│   ├── Accounting.Api/                  # REST/gRPC endpoints and controllers
│   ├── Accounting.AppHost/              # .NET Aspire orchestration host
│   ├── Accounting.BackOffice/           # Background jobs and event handlers
│   ├── Accounting.BackOffice.Orleans/   # Orleans grains for stateful processing
│   ├── Accounting.Contracts/            # Integration events and shared models
│   └── Accounting/                      # Core domain logic (commands, queries, entities)
├── test/
│   └── Accounting.Tests/                # Integration and architecture tests
├── infra/
│   └── Accounting.Database/             # Liquibase database migrations
└── docs/                               # DocFX documentation
```

### Service Components

- **Accounting.Api** - REST and gRPC endpoints for ledger and business day management
- **Accounting.BackOffice** - Background processing service for integration events  
- **Accounting.BackOffice.Orleans** - Stateful ledger processing using Orleans actors
- **Accounting.Contracts** - Shared models and integration events for cross-service communication
- **Accounting** (Core) - Domain entities, commands, queries, and business logic
- **Accounting.AppHost** - .NET Aspire orchestration with service discovery
- **Accounting.Tests** - Comprehensive testing with Testcontainers integration

## Key Features

### Domain-Driven Design
- **Entities**: Ledger domain models with audit trails
- **Commands**: Create operations with FluentValidation and source generation
- **Queries**: Paginated retrieval with Dapper integration
- **Integration Events**: Cross-service communication events

### Technology Stack
- **.NET 9** with latest C# features
- **WolverineFx** for CQRS and messaging
- **Orleans** for stateful ledger processing
- **PostgreSQL 17** with Liquibase migrations
- **gRPC** with Protocol Buffers for inter-service communication
- **Testcontainers** for integration testing with real databases

### Source Generation
Custom source generators reduce boilerplate with attributes like:
```csharp
[DbCommand(sp: "accounting.create_ledger", nonQuery: true)]
public partial record CreateLedgerCommand(string Name, string? Type) : ICommand<Guid>;
```

### Testing Strategy
- **Unit Tests**: Mock-based testing with NSubstitute
- **Integration Tests**: End-to-end testing with real PostgreSQL via Testcontainers
- **Architecture Tests**: NetArchTest enforcement of layering rules
- **Database Tests**: Direct stored procedure testing

## Port Configuration

The Accounting service uses the following port allocation:

### Service Ports (8120-8139)
- **8121**: Accounting.Api (HTTP)
- **8131**: Accounting.Api (HTTPS)
- **8122**: Accounting.Api (gRPC)
- **8123**: Accounting.BackOffice (HTTP)
- **8133**: Accounting.BackOffice (HTTPS)
- **8124**: Accounting.BackOffice.Orleans (HTTP)
- **8134**: Accounting.BackOffice.Orleans (HTTPS)
- **8139**: Documentation Service

### Aspire Dashboard
- **18120**: Aspire Dashboard (HTTP)
- **18130**: Aspire Dashboard (HTTPS)
- **8120**: Aspire Resource Service (HTTP)
- **8130**: Aspire Resource Service (HTTPS)

### Shared Services
- **5432**: PostgreSQL
- **4317/4318**: OpenTelemetry OTLP

## Prerequisites

- .NET 9 SDK
- PostgreSQL running on localhost:5432 (username: `postgres`, password: `password@`)
- Docker (optional, for containerized deployment)
- Liquibase CLI (for manual database setup)

## Quick Start

### Option 1: .NET Aspire (Recommended)

The fastest way to get started is using .NET Aspire orchestration:

```bash
# Run the entire Accounting service stack
cd Accounting/src/Accounting.AppHost
dotnet run
```

This automatically:
- ✅ Sets up PostgreSQL databases with Liquibase
- ✅ Starts all services (API, BackOffice, Orleans)
- ✅ Configures service discovery and dependencies
- ✅ Provides observability dashboard

**Access Points:**
- **Aspire Dashboard**: http://localhost:18120
- **Accounting API**: http://localhost:8121/swagger
- **Documentation**: http://localhost:8139

### Option 2: Manual Setup

For full control over the setup process:

#### 1. Database Setup
Run these commands from the `Accounting/infra/Accounting.Database/` directory:

```bash
cd Accounting/infra/Accounting.Database/

# Step 1: Setup databases
liquibase update --defaults-file liquibase.setup.properties

# Step 2: Service bus schema  
liquibase update --defaults-file liquibase.servicebus.properties

# Step 3: Domain schema
liquibase update
```

#### 2. Run Individual Services

```bash
# Terminal 1 - API service
dotnet run --project Accounting/src/Accounting.Api

# Terminal 2 - Background service
dotnet run --project Accounting/src/Accounting.BackOffice

# Terminal 3 - Orleans service
dotnet run --project Accounting/src/Accounting.BackOffice.Orleans
```

#### 3. Verify Setup
- **API Health**: http://localhost:8121/health
- **Swagger UI**: http://localhost:8121/swagger
- **gRPC**: Connect to localhost:8122

### Option 3: Docker Compose

For containerized deployment:

```bash
docker-compose up --build
```

## API Usage

### REST API Examples

```bash
# Create a ledger
curl -X POST http://localhost:8121/ledgers \
  -H "Content-Type: application/json" \
  -d '{"name": "General Ledger", "type": "Asset"}'

# Get all ledgers (paginated)
curl http://localhost:8121/ledgers?pageNumber=1&pageSize=10

# Get specific ledger
curl http://localhost:8121/ledgers/{id}
```

### gRPC API
Use the Protocol Buffer definitions in `src/Accounting.Api/Ledgers/Protos/` for type-safe client generation.

## Database Schema

### Tables
- **ledger_balances**: Ledger balance information with audit fields
- **ledger_currencies**: Multi-currency support

### Migration Management
Liquibase handles schema evolution with:
- **Version Control**: All changes tracked in Git
- **Rollback Support**: Safe rollback capabilities
- **Environment Promotion**: Consistent schema across environments

## Development and Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test categories
dotnet test Accounting/test/Accounting.Tests --filter Category=Integration
dotnet test Accounting/test/Accounting.Tests --filter Category=Unit
```

### Test Categories
1. **Unit Tests**: Fast, isolated component tests with mocks
2. **Integration Tests**: Service-level tests with real PostgreSQL via Testcontainers
3. **Architecture Tests**: Enforce DDD layering and dependencies
4. **Database Tests**: Direct stored procedure and database integration testing

### Test Infrastructure
- **Testcontainers**: Real PostgreSQL 17-alpine for integration tests
- **WebApplicationFactory**: In-memory test servers for API testing
- **Liquibase Migration**: Automated database setup in test containers
- **Docker Networks**: Proper container communication during tests

## Documentation

### Building Documentation

This service includes comprehensive documentation built with [DocFX](https://dotnet.github.io/docfx/) using the [Material theme](https://ovasquez.github.io/docfx-material/).

#### Using Docker
Run from the **Accounting** folder (not the docs folder):

```bash
# Build the image (from Accounting folder)
docker build -f docs/Dockerfile -t accounting-docfx .

# Run the container
docker run -d -p 8139:8080 --name accounting-docs accounting-docfx
```

#### Using Local DocFX
Run from the **Accounting** folder:

```bash
# Install DocFX (if not already installed)
dotnet tool install -g docfx

# Serve documentation (from Accounting folder)
docfx docs/docfx.json --serve -p 8139 -n "*"
```

#### Using .NET Aspire
The documentation service is automatically included when running the Accounting AppHost:

```bash
cd src/Accounting.AppHost
dotnet run
```

The documentation will be available in the Aspire dashboard with a direct link.

### Accessing the Documentation

Once running, the documentation is available at:
- **Local**: http://localhost:8139

### Documentation Structure
```
docs/
├── content/              # Markdown documentation files
│   ├── architecture.md   # Detailed architectural patterns
│   ├── api-reference.md  # Complete REST and gRPC API docs
│   ├── database.md       # Database schema and migrations
│   └── toc.yml          # Table of contents
├── templates/            # DocFX Material theme
├── images/              # Documentation assets
├── docfx.json          # DocFX configuration
└── index.md            # Documentation homepage
```

## Integration Events

The Accounting service communicates with other services through integration events:

### Published Events
- **LedgerCreatedEvent**: Emitted when a new ledger is created
- **LedgerUpdatedEvent**: Emitted when ledger information changes
- **BusinessDayEnded**: Emitted when business day operations complete

### Consumed Events
- Events from other services that affect accounting operations

## Orleans Integration

The Accounting.BackOffice.Orleans service provides stateful ledger processing:

### Features
- **Ledger Grains**: Stateful actors for ledger lifecycle management
- **3 Replicas**: High availability with Orleans clustering
- **Direct API**: HTTP endpoints for ledger operations
- **Orleans Dashboard**: Real-time grain monitoring

## Monitoring and Observability

### Health Checks
- **API Health**: `/health` endpoint with dependency checks
- **Service Dependencies**: Database connectivity validation
- **Orleans Health**: Grain health monitoring

### Logging
- **Serilog**: Structured logging with correlation IDs
- **OpenTelemetry**: Distributed tracing across services
- **Integration**: XUnit sink for test output

### Metrics
- **Custom Metrics**: Domain-specific business metrics
- **Performance**: Database operation timing
- **Orleans Metrics**: Grain activation and processing metrics

## Additional Resources

- **[Architecture Documentation](docs/content/architecture.md)** - Detailed design patterns
- **[API Reference](docs/content/api-reference.md)** - Complete API documentation  
- **[Database Schema](docs/content/database.md)** - Database design and migrations
- **[CLAUDE.md](../CLAUDE.md)** - AI assistant development guidance