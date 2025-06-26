# Billing Service

The Billing Service manages cashiers, invoices, and payment processing within the broader Operations platform. It provides both REST and gRPC APIs for managing billing operations and integrates with other services through event-driven messaging.

## Code Structure and Design Philosophy

### Overview

This codebase is intentionally structured to mirror the real-world operations and organizational structure of a billing department.
Each part of the code corresponds or should correspond directly to a real-world role or operation, ensuring that the code remains 100% product-oriented and easy to understand.
The idea is that main operations/actions would be recognizable by a non-technical product person.

### Real-World Mirroring

For instance, if the billing department handles creating invoices, the code includes a clear and
direct set of actions to handle invoice creation.
Smaller tasks, or sub-actions, needed to complete a main action are also represented in a similar manner.
If a sub-action is only used within one main action, it remains nested inside that operation. If it needs to be reused by multiple operations,
it is extracted and made reusable, but still mirroring the real-world scenario.

### Avoiding Unnecessary Abstractions

This design philosophy avoids unnecessary abstractions. There are no additional layers like repositories or services unless they represent
something that exists in the real department. Infrastructure elements like logging or authorization are present as they support the system’s
functionality, same as water pipes and electricity support a billing department office. Even the database is viewed as a digital parallel to a
real-world archive or filing system.

### No "Domain" Objects

A key principle is the absence of smart objects. This means that an invoice, for example, is not an object that can change itself.
Instead, it is simply treated as a digital record, and all modifications are performed by "external" actors (something is changing the invoice,
the invoice does not change itself). This ensures that the code reflects digital representations of real-world entities and processes,
rather than trying to replicate objects with their own behaviors.

### Synchronous and Asynchronous Operations

The codebase also distinguishes between synchronous and asynchronous operations.
The API represents the front office of the billing department, handling synchronous operations where immediate responses are expected.
In contrast, the back office is represented by asynchronous operations that do not require immediate responses, allowing for efficient,
behind-the-scenes processing.

## What is the Billing Service?

The Billing Service is part of a .NET 9 microservices system built using Domain-Driven Design principles. It provides a complete billing solution with modern web UI and comprehensive testing. It handles:

- **Cashier Management**: Create and manage cashiers with multi-currency support
- **Invoice Processing**: Handle invoice lifecycle with Orleans-based stateful processing
- **Payment Integration**: Process payments and emit integration events
- **Cross-Service Integration**: React to business events from other services like Accounting
- **Modern Web UI**: SvelteKit-based responsive web application with gRPC integration
- **Comprehensive Testing**: Unit, integration, and end-to-end testing with real browsers

## Service Architecture

The Billing service follows the standard microservices structure with clean separation of concerns:

```
Billing/
├── docs/                            # DocFX documentation system
├── infra/                           # Infrastructure and database
│   └── Billing.Database/            # Liquibase Database project
├── src/                             # Source code projects
│   ├── Billing/                     # Domain logic
│   ├── Billing.Api/                 # REST/gRPC endpoints
│   ├── Billing.AppHost/             # .NET Aspire orchestration
│   ├── Billing.BackOffice/          # Background processing
│   ├── Billing.BackOffice.Orleans/  # Orleans stateful processing
│   └── Billing.Contracts/           # Integration events and models
├── test/                            # Testing projects
│   └── Billing.Tests/
│       ├── Architecture/            # NetArchTest architecture validation
│       ├── Integration/             # Integration tests with Testcontainers
│       └── Unit/                    # Unit tests with mocking
└── web/                             # Frontend applications
    └── billing-ui/                  # Web application
```

### Service Components

#### **Backend Services**
- **Billing.Api** - Dual-protocol API (REST + gRPC) for cashier and invoice management
- **Billing.BackOffice** - Event-driven background processing with Wolverine
- **Billing.BackOffice.Orleans** - Stateful invoice processing using Orleans actors (3-replica cluster)
- **Billing.Contracts** - Integration events and shared models for cross-service communication
- **Billing** (Core) - Domain entities, commands, queries, and business logic with DDD patterns
- **Billing.AppHost** - .NET Aspire orchestration with comprehensive service discovery

#### **Frontend & Testing**
- **billing-ui** - Modern SvelteKit web application with Tailwind CSS and shadcn-svelte components
- **Billing.Tests** - Multi-layered testing with Testcontainers, architecture tests, and Playwright E2E

#### **Infrastructure & Documentation**
- **Billing.Database** - Liquibase-based database management with multi-database pattern
- **docs/** - DocFX documentation system with custom theme and containerized deployment

## Key Features

### Domain-Driven Design
- **Entities**: Cashier and Invoice domain models with audit trails
- **Commands**: Create operations with FluentValidation and source generation
- **Queries**: Paginated retrieval with Dapper integration
- **Integration Events**: Cross-service communication events

### Technology Stack

#### **Backend Stack**
- **.NET 9** with latest C# features and nullable reference types
- **WolverineFx** for CQRS, event sourcing, and messaging
- **Orleans** for stateful actor-based processing with Azure Storage clustering
- **PostgreSQL 17** with Liquibase migrations and multi-database architecture
- **gRPC** with Protocol Buffers for type-safe inter-service communication
- **OpenTelemetry** for distributed tracing and observability

#### **Frontend Stack**
- **SvelteKit** with Svelte 5 and TypeScript for modern reactive UI
- **Tailwind CSS** with shadcn-svelte component library for responsive design
- **gRPC-Web** with `@grpc/grpc-js` for direct backend communication
- **Lucide Svelte** for consistent iconography

#### **Testing & Quality**
- **Testcontainers** for integration testing with real PostgreSQL containers
- **Playwright** for end-to-end browser testing with accessibility validation
- **NetArchTest** for architecture rule enforcement
- **NSubstitute + Shouldly** for unit testing with fluent assertions

### Source Generation
Custom source generators reduce boilerplate DB access code with attributes like:
```csharp
[DbCommand(sp: "billing.create_cashier", nonQuery: true)]
public partial record CreateCashierCommand(string Name, string? Email) : ICommand<Guid>;
```

### Testing Strategy
- **Unit Tests**: Mock-based testing with NSubstitute
- **Integration Tests**: End-to-end testing with real PostgreSQL via Testcontainers
- **Architecture Tests**: NetArchTest enforcement of layering rules

## Port Configuration

The Billing service uses the following port allocation:

### Aspire Dashboard
- **Aspire Dashboard:** 18100 (HTTP) / 18110 (HTTPS)
- **Aspire Resource Service:** 8100 (HTTP) / 8110 (HTTPS)

### Service Ports (8100-8119)
- **Billing.UI:** 8105 (HTTP) / 8115 (HTTPS)
- **Billing.Api:** 8101 (HTTP) / 8111 (HTTPS) / 8102 (gRPC insecure)
- **Billing.BackOffice:** 8103 (HTTP) / 8113 (HTTPS)
- **Billing.BackOffice.Orleans:** 8104 (HTTP) / 8114 (HTTPS)
- **Documentation Service:** 8119

### Shared Services
- **5432**: PostgreSQL
- **4317/4318**: OpenTelemetry OTLP

## Prerequisites

- Docker (optional, for containerized deployment) or..
- .NET 9 SDK
- PostgreSQL running on localhost:5432 (username: `postgres`, password: `password@`)
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
- ✅ Starts all services (UI, API, BackOffice, Orleans)
- ✅ Configures service discovery and dependencies
- ✅ Provides observability dashboard

**Access Points:**
- **Aspire Dashboard**: http://localhost:18100
- **Billing Web UI**: http://localhost:8105
- **Billing API**: http://localhost:8101/scalar
- **Orleans Dashboard**: http://localhost:8104/dashboard
- **Documentation**: http://localhost:8119

### Option 2: Manual Setup

For full control over the setup process:

#### 1. Database Setup
Run these commands from the `Billing/infra/Billing.Database/` directory:

```bash
cd Billing/infra/Billing.Database/

# Step 1: Setup databases (creates actual dbs)
liquibase update --defaults-file liquibase.setup.properties

# Step 2: Service bus schema
liquibase update --defaults-file liquibase.servicebus.properties

# Step 3: Application schema
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
- **OpenAPI UI**: http://localhost:8101/scalar
- **gRPC**: Connect to localhost:8102

### Option 3: Docker Compose

For containerized deployment:

```bash
docker compose up --build
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

```bash
# Connect with grpcurl
grpcurl -plaintext localhost:8102 list
grpcurl -plaintext -d '{"pageNumber": 1, "pageSize": 10}' localhost:8102 cashiers.CashiersService/GetCashiers
```

### Web UI Access
The Billing service includes a modern SvelteKit web application:

- **URL**: http://localhost:8105 (when running via Aspire)
- **Features**: Responsive design, accessibility-compliant, gRPC integration
- **Pages**: Dashboard, Cashier management, Invoice processing

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

#### Backend Tests (C#)
```bash
# Run all backend tests
dotnet test

# Run specific test categories (TODO needs to be updated)
dotnet test Billing/test/Billing.Tests --filter Category=Integration
dotnet test Billing/test/Billing.Tests --filter Category=Unit
dotnet test Billing/test/Billing.Tests --filter Category=Architecture
```

#### Frontend Tests (Playwright)
```bash
# Navigate to UI project
cd Billing/web/billing-ui

# Install dependencies
npm install

# Run UI tests (requires backend running on port 8102)
npm run test:ui

# Run UI tests with mock API (standalone)
npm run test:mock

# Run tests in interactive mode
npm run test:ui-mode

# Run tests with browser visible
npm run test:ui-headed
```

**Important**: Backend must be running on gRPC port 8102 for full UI tests. Use `npm run test:mock` for tests without backend dependency.

### Test Categories

#### Backend Tests
1. **Unit Tests**: Fast, isolated component tests with NSubstitute mocks
2. **Integration Tests**: Service-level tests with real PostgreSQL, and dependent services via Testcontainers
3. **Architecture Tests**: NetArchTest enforcement of DDD layering and dependencies

#### Frontend Tests (9 Test Suites)
1. **Functional Tests**: Core cashier CRUD operations (`cashiers.spec.ts`)
2. **Form Validation**: Input validation and error handling (`cashier-form.spec.ts`)
3. **Navigation Tests**: Routing and menu functionality (`navigation.spec.ts`)
4. **Accessibility Tests**: WCAG compliance and keyboard navigation (`accessibility.spec.ts`)
5. **Integration Tests**: Full backend communication (`integration.spec.ts`)
6. **Mock API Tests**: Standalone testing without backend (`mock-api.spec.ts`)
7. **Responsive Tests**: Multi-viewport testing (`responsive.spec.ts`)
8. **Performance Tests**: Load time and memory usage (`performance.spec.ts`)
9. **Error Handling**: Network failures and edge cases (`error-handling.spec.ts`)

### Test Infrastructure
- **Testcontainers**: Real PostgreSQL 17-alpine for integration tests
- **Liquibase Migration**: Automated database setup in test containers

## Documentation

### Building Documentation

This service includes comprehensive documentation built with [DocFX](https://dotnet.github.io/docfx/).

#### Using Docker
Run from the **Billing** folder (not the docs folder):

```bash
# Build the image (from Billing folder)
docker build -f docs/Dockerfile -t billing-docfx .

# Run the container
docker run -d -p 8119:8080 --name billing-docs billing-docfx docs/docfx.json --serve -p 8119 // TODO: this is missing a volume mapping see aspire
```

#### Using Local DocFX
Run from the **Billing** folder:

```bash
# Install DocFX (if not already installed)
dotnet tool install -g docfx

# Serve documentation (from Billing folder)
docfx docs/docfx.json --serve -p 8119 -n "*"
```

**Working Directory**: All DocFX commands should be run from the **root** folder, not the `docs` folder. This allows DocFX to properly access the source code for API documentation generation.

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
│   ├── toc.yml           # Table of contents
│   └── ...
├── templates/            # DocFX theme
├── images/               # Documentation assets
├── docfx.json            # DocFX configuration
└── index.md              # Documentation homepage (Link to README.md)
```

### Important Notes

## Integration Events

The Billing service communicates with other services through integration events:

### Published Events
- **CashierCreatedEvent**: Emitted when a new cashier is created
- **CashierUpdatedEvent**: Emitted when cashier information changes
- **InvoicePaidEvent**: Emitted when an invoice payment is processed

### Consumed Events
- **BusinessDayEndedEvent**: Reacts to accounting business day operations

## Web UI Integration

The Billing service includes a modern SvelteKit web application with full-stack capabilities:

### UI Architecture
- **Framework**: SvelteKit with Svelte 5 runes for reactive state management
- **Styling**: Tailwind CSS with shadcn-svelte component library
- **Communication**: Direct gRPC communication with `@grpc/grpc-js`
- **State Management**: Svelte stores with TypeScript for type safety
- **Deployment**: Integrated with .NET Aspire for seamless development

### Key Features
- **SSR + SPA**: Server-side rendering with client-side hydration
- **Responsive Design**: Mobile-first design with Tailwind breakpoints
- **Accessibility**: WCAG-compliant with keyboard navigation and screen reader support
- **Type Safety**: Full TypeScript integration with gRPC-generated types
- **Error Boundaries**: Graceful error handling with fallback UI
- **Progressive Enhancement**: Works without JavaScript for core functionality

### UI Pages
- **Dashboard** (`/`) - Service module overview with navigation cards
- **Cashiers List** (`/cashiers`) - Paginated cashier management with search
- **Create Cashier** (`/cashiers/create`) - Form with validation and multi-currency support
- **Cashier Details** (`/cashiers/[id]`) - Individual cashier view with edit capabilities
- **Edit Cashier** (`/cashiers/[id]/edit`) - Update form with optimistic updates

### Development Commands
```bash
cd Billing/web/billing-ui

# Install dependencies
npm install

# Development server
npm run dev

# Type checking
npm run check

# Build for production
npm run build
```

## Orleans Integration

The Billing.BackOffice.Orleans service provides stateful invoice processing:

### Features
- **Invoice Grains**: Stateful actors for invoice lifecycle management
- **3 Replicas**: High availability with Orleans clustering and Azure Storage
- **Direct API**: HTTP endpoints for invoice operations
- **Orleans Dashboard**: Real-time grain monitoring at `/dashboard`

### Invoice Operations
- **POST** `/invoices/{id}/pay` - Process invoice payment
- **GET** `/invoices/{id}` - Retrieve invoice state
- **Dashboard**: http://localhost:8104/dashboard - Grain health and performance monitoring

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
