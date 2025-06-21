# Billing Service

The Billing Service manages cashiers, invoices, and payment processing within the broader Operations platform. It provides both REST and gRPC APIs for managing billing operations and integrates with other services through event-driven messaging.

## What is the Billing Service?

The Billing Service is part of a .NET 9 microservices system built using Domain-Driven Design principles. It handles:

- **Cashier Management**: Create and manage cashiers with multi-currency support
- **Invoice Processing**: Handle invoice lifecycle with Orleans-based stateful processing  
- **Payment Integration**: Process payments and emit integration events
- **Cross-Service Integration**: React to business events from other services like Accounting

## Service Architecture

The Billing service follows the standard microservices structure with clean separation of concerns:

```
Billing/
├── src/
│   ├── Billing.Api/                  # REST/gRPC endpoints and controllers
│   ├── Billing.AppHost/              # .NET Aspire orchestration host
│   ├── Billing.BackOffice/           # Background jobs and event handlers
│   ├── Billing.BackOffice.Orleans/   # Orleans grains for stateful processing
│   ├── Billing.Contracts/            # Integration events and shared models
│   └── Billing/                      # Core domain logic (commands, queries, entities)
├── test/
│   └── Billing.Tests/                # Integration and architecture tests
├── infra/
│   └── Billing.Database/             # Liquibase database migrations
└── docs/                             # DocFX documentation
```

### Service Components

- **Billing.Api** - REST and gRPC endpoints for cashier and invoice management
- **Billing.BackOffice** - Background processing service for integration events  
- **Billing.BackOffice.Orleans** - Stateful invoice processing using Orleans actors
- **Billing.Contracts** - Shared models and integration events for cross-service communication
- **Billing** (Core) - Domain entities, commands, queries, and business logic
- **Billing.AppHost** - .NET Aspire orchestration with service discovery
- **Billing.Tests** - Comprehensive testing with Testcontainers integration

## Key Features

### Domain-Driven Design
- **Entities**: Cashier and Invoice domain models with audit trails
- **Commands**: Create operations with FluentValidation and source generation
- **Queries**: Paginated retrieval with Dapper integration
- **Integration Events**: Cross-service communication events

### Technology Stack
- **.NET 9** with latest C# features
- **WolverineFx** for CQRS and messaging
- **Orleans** for stateful invoice processing
- **PostgreSQL 17** with Liquibase migrations
- **gRPC** with Protocol Buffers for inter-service communication
- **Testcontainers** for integration testing with real databases

### Source Generation
Custom source generators reduce boilerplate with attributes like:
```csharp
[DbCommand(sp: "billing.create_cashier", nonQuery: true)]
public partial record CreateCashierCommand(string Name, string? Email) : ICommand<Guid>;
```

### Testing Strategy
- **Unit Tests**: Mock-based testing with NSubstitute
- **Integration Tests**: End-to-end testing with real PostgreSQL via Testcontainers
- **Architecture Tests**: NetArchTest enforcement of layering rules
- **Database Tests**: Direct stored procedure testing

## Port Configuration

The Billing service uses the following port allocation:

### Service Ports (8100-8119)
- **8101**: Billing.Api (HTTP)
- **8111**: Billing.Api (HTTPS)
- **8102**: Billing.Api (gRPC)
- **8103**: Billing.BackOffice (HTTP)
- **8113**: Billing.BackOffice (HTTPS)
- **8104**: Billing.BackOffice.Orleans (HTTP)
- **8114**: Billing.BackOffice.Orleans (HTTPS)
- **8119**: Documentation Service

### Aspire Dashboard
- **18100**: Aspire Dashboard (HTTP)
- **18110**: Aspire Dashboard (HTTPS)
- **8100**: Aspire Resource Service (HTTP)
- **8110**: Aspire Resource Service (HTTPS)

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
# Run the entire Billing service stack
cd Billing/src/Billing.AppHost
dotnet run
```

This automatically:
- ✅ Sets up PostgreSQL databases with Liquibase
- ✅ Starts all services (API, BackOffice, Orleans)
- ✅ Configures service discovery and dependencies
- ✅ Provides observability dashboard

**Access Points:**
- **Aspire Dashboard**: http://localhost:18100
- **Billing API**: http://localhost:8101/swagger
- **Documentation**: http://localhost:8119

### Option 2: Manual Setup

For full control over the setup process:

#### 1. Database Setup
Run these commands from the `Billing/infra/Billing.Database/` directory:

```bash
cd Billing/infra/Billing.Database/

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
dotnet run --project Billing/src/Billing.Api

# Terminal 2 - Background service
dotnet run --project Billing/src/Billing.BackOffice

# Terminal 3 - Orleans service
dotnet run --project Billing/src/Billing.BackOffice.Orleans
```

#### 3. Verify Setup
- **API Health**: http://localhost:8101/health
- **Swagger UI**: http://localhost:8101/swagger
- **gRPC**: Connect to localhost:8102

### Option 3: Docker Compose

For containerized deployment:

```bash
docker-compose up --build
```

## API Usage

### REST API Examples

```bash
# Create a cashier
curl -X POST http://localhost:8101/cashiers \
  -H "Content-Type: application/json" \
  -d '{"name": "John Doe", "email": "john@example.com"}'

# Get all cashiers (paginated)
curl http://localhost:8101/cashiers?pageNumber=1&pageSize=10

# Get specific cashier
curl http://localhost:8101/cashiers/{id}

# Get invoices
curl http://localhost:8101/invoices
```

### gRPC API
Use the Protocol Buffer definitions in `src/Billing.Api/Cashier/Protos/` for type-safe client generation.

## Database Schema

### Tables
- **cashiers**: Core cashier information with audit fields
- **cashier_currencies**: Multi-currency support
- **invoices**: Invoice management

### Stored Procedures
- **billing.create_cashier**: Cashier creation with business rules

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
dotnet test Billing/test/Billing.Tests --filter Category=Integration
dotnet test Billing/test/Billing.Tests --filter Category=Unit
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
Run from the **Billing** folder (not the docs folder):

```bash
# Build the image (from Billing folder)
docker build -f docs/Dockerfile -t billing-docfx .

# Run the container
docker run -d -p 8119:8080 --name billing-docs billing-docfx
```

#### Using Local DocFX
Run from the **Billing** folder:

```bash
# Install DocFX (if not already installed)
dotnet tool install -g docfx

# Serve documentation (from Billing folder)
docfx docs/docfx.json --serve -p 8119 -n "*"
```

#### Using .NET Aspire
The documentation service is automatically included when running the Billing AppHost:

```bash
cd src/Billing.AppHost
dotnet run
```

The documentation will be available in the Aspire dashboard with a direct link.

### Accessing the Documentation

Once running, the documentation is available at:
- **Local**: http://localhost:8119

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

### Important Notes

**Working Directory**: All DocFX commands should be run from the **Billing** folder, not the `docs` folder. This allows DocFX to properly access the source code for API documentation generation.

**Correct**:
```bash
cd Billing/
docfx docs/docfx.json --serve -p 8119 -n "*"
```

**Incorrect**:
```bash
cd Billing/docs/
docfx docfx.json --serve -p 8119 -n "*"  # Won't find source code
```

## Integration Events

The Billing service communicates with other services through integration events:

### Published Events
- **CashierCreatedEvent**: Emitted when a new cashier is created
- **CashierUpdatedEvent**: Emitted when cashier information changes
- **InvoicePaidEvent**: Emitted when an invoice payment is processed

### Consumed Events
- **BusinessDayEndedEvent**: Reacts to accounting business day operations

## Orleans Integration

The Billing.BackOffice.Orleans service provides stateful invoice processing:

### Features
- **Invoice Grains**: Stateful actors for invoice lifecycle management
- **3 Replicas**: High availability with Orleans clustering
- **Direct API**: HTTP endpoints for invoice operations
- **Orleans Dashboard**: Real-time grain monitoring

### Invoice Operations
- **POST** `/invoices/{id}/pay` - Process invoice payment
- **GET** `/invoices/{id}` - Retrieve invoice state

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

## Troubleshooting

### Common Issues

#### Database Connection Errors
```bash
# Check PostgreSQL is running
netstat -tulpn | grep 5432

# Verify database exists
psql -h localhost -U postgres -l
```

#### Port Conflicts
```bash
# Check if ports are in use
netstat -tulpn | grep 8101
netstat -tulpn | grep 8119
```

#### Container Issues
```bash
# Check container logs
docker logs billing-docs

# Check container status
docker ps -a
```

#### Test Failures
- Ensure Docker is running for Testcontainers
- Check PostgreSQL is available on localhost:5432
- Verify Liquibase migrations are current

### Documentation Issues

#### Documentation not updating
- Rebuild the Docker image after making changes
- Clear browser cache
- Check that markdown files are valid

#### API documentation missing
- Ensure source code has XML documentation comments
- Check that project references are correct in `docfx.json`
- Verify build succeeds without errors

#### Theme not loading
- Check that `docfx.json` references the correct template path
- Verify theme files are not corrupted

## Contributing

### Adding New Features
1. Follow DDD patterns in the core domain
2. Add integration tests for new functionality
3. Update documentation in `docs/content/`
4. Ensure all tests pass

### Documentation Updates
1. Add new markdown files to `content/` directory
2. Update table of contents in `toc.yml`
3. Test changes locally with DocFX
4. Update this README if adding new capabilities

### Database Changes
1. Create new Liquibase changesets
2. Test migrations in local environment
3. Update database documentation
4. Ensure rollback scripts are available

## Additional Resources

- **[Architecture Documentation](docs/content/architecture.md)** - Detailed design patterns
- **[API Reference](docs/content/api-reference.md)** - Complete API documentation  
- **[Database Schema](docs/content/database.md)** - Database design and migrations
- **[CLAUDE.md](../CLAUDE.md)** - AI assistant development guidance