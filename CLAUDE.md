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

### Docker Build Commands
```bash
# Build individual service images
docker build -t billing-api -f Billing/src/Billing.Api/Dockerfile .
docker build -t billing-backoffice -f Billing/src/Billing.BackOffice/Dockerfile .
docker build -t accounting-api -f Accounting/src/Accounting.Api/Dockerfile .
docker build -t accounting-backoffice -f Accounting/src/Accounting.BackOffice/Dockerfile .

# Run individual containers
docker run -p 8101:8080 billing-api
docker run -p 8121:8080 accounting-api
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

## Port Configuration

This system uses a structured port allocation pattern optimized for macOS compatibility:

### **Service Port Ranges**
- **Billing**: 8100-8119 (20 ports)
- **Accounting**: 8120-8139 (20 ports)  
- **Operations**: 8140-8159 (20 ports)

### **Port Pattern Within Each Service**
```
XX00: Aspire Resource Service (HTTP)
XX01: Main API (HTTP)
XX02: Main API (gRPC)
XX03: BackOffice (HTTP)
XX04: Orleans (HTTP)
XX10: Aspire Resource Service (HTTPS)
XX11: Main API (HTTPS)
XX13: BackOffice (HTTPS)
XX14: Orleans (HTTPS)
XX19: Documentation (last port of range)
```

### **Aspire Dashboard Ports**
- **HTTP**: Service base + 10,000 (e.g., 8100 → 18100)
- **HTTPS**: Service base + 10,010 (e.g., 8100 → 18110)

### **Current Service Ports**

#### **Billing Service (8100-8119)**
- Aspire Dashboard: 18100 (HTTP), 18110 (HTTPS)
- Aspire Resource: 8100 (HTTP), 8110 (HTTPS)
- API: 8101 (HTTP), 8111 (HTTPS), 8102 (gRPC)
- BackOffice: 8103 (HTTP), 8113 (HTTPS)
- Orleans: 8104 (HTTP), 8114 (HTTPS)
- Documentation: 8119

#### **Accounting Service (8120-8139)**
- Aspire Dashboard: 18120 (HTTP), 18130 (HTTPS)
- Aspire Resource: 8120 (HTTP), 8130 (HTTPS)
- API: 8121 (HTTP), 8131 (HTTPS), 8122 (gRPC)
- BackOffice: 8123 (HTTP), 8133 (HTTPS)
- Orleans: 8124 (HTTP), 8134 (HTTPS)
- Documentation: 8139 (reserved)

#### **Operations Service (8140-8159)**
- Aspire Dashboard: 18140 (HTTP), 18150 (HTTPS)
- Aspire Resource: 8140 (HTTP), 8150 (HTTPS)
- Documentation: 8159 (reserved)

### **Shared Services (Standard Ports)**
- **PostgreSQL**: 5432
- **OpenTelemetry**: 4317 (HTTPS), 4318 (HTTP)

## API Testing

### Docker Compose Testing (Billing Service)
```bash
# Start billing services with database migrations
docker compose -f docker-compose.billing.yml up -d

# Wait for services to start (about 10 seconds)
sleep 10

# Test API - Create a cashier
curl -X POST http://localhost:5061/cashiers \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Doe",
    "email": "john.doe@example.com", 
    "currencies": ["USD", "EUR"]
  }'

# Verify in database
docker exec billing-db psql -U postgres -d billing -c "SELECT * FROM billing.cashiers;"

# Clean up
docker compose -f docker-compose.billing.yml down
```

### Manual API Testing
```bash
# Create cashier
curl -X POST http://localhost:5061/cashiers \
  -H "Content-Type: application/json" \
  -d '{"name": "Jane Smith", "email": "jane@example.com", "currencies": ["USD"]}'

# Get cashiers
curl http://localhost:5061/cashiers

# Create invoice
curl -X POST http://localhost:5061/invoices \
  -H "Content-Type: application/json" \
  -d '{"cashierId": "CASHIER_ID", "amount": 100.00, "currency": "USD"}'
```

## UI Integration

### **Frontend Architecture**
- **SvelteKit UI** located at `Billing/web/billing-ui/`
- **gRPC Communication** with Billing API using `@grpc/grpc-js`
- **Integrated with Aspire** orchestration for development
- **Responsive Design** with Tailwind CSS and Lucide icons

### **Aspire Integration**
```csharp
// In Billing.AppHost/Program.cs
var billingUi = builder
    .AddNpmApp("billing-ui", "../../../Billing/web/billing-ui")
    .WithReference(billingApi)
    .WithEnvironment("GRPC_HOST", () => billingApi.GetEndpoint("grpc").Host)
    .WithEnvironment("GRPC_PORT", () => billingApi.GetEndpoint("grpc").Port.ToString())
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();
```

### **UI Development Commands**
```bash
# Install UI dependencies
cd Billing/web/billing-ui && npm install

# Run UI in development mode
npm run dev

# Build UI for production
npm run build

# Run UI tests (requires backend to be running - see below)
npm run test:ui

# Run UI tests with mocked API (standalone, no backend required)
npm run test:mock
```

### **Backend Setup for UI Tests**
UI Playwright tests require the backend gRPC API to be running. The UI tests use the gRPC API only.

```bash
# Method 1: Run full Aspire orchestration (recommended)
cd Billing/src/Billing.AppHost
dotnet run

# Method 2: Run API standalone for testing
cd Billing/src/Billing.Api
dotnet run --launch-profile http
# This runs HTTP on port 8101 and gRPC on port 8102

# Method 3: Using Docker Compose (minimal setup)
docker compose -f Billing/compose.yml up billing-api -d
```

**Important:** 
- All Playwright tests should use `npm run test:ui` as the command prefix
- Backend must be running on port 8102 (gRPC) for UI tests to pass
- Use `npm run test:mock` for tests without backend dependency

## Recent Updates

### **Stored Procedures Refactored (2024)**
- Changed naming pattern from `action_resource` to `resource_action`
- Updated procedures: `cashier_create`, `cashier_update`, `cashier_delete`, `invoice_create`, `invoice_update`, `invoice_cancel`
- All C# references updated to match new procedure names

### **OpenAPI Documentation Added**
- Comprehensive documentation for all controllers with XML summaries
- Response types and error codes properly documented
- Tags added for better API organization using Microsoft OpenAPI extensions

### **Result Pattern Implementation**
- `GetInvoice` query updated to use Result pattern instead of exceptions
- Uses OneOf library with ValidationFailure for error handling
- Consistent error handling across query operations

### **Integration Events Refactored**
- Removed "Event" suffix from all integration events
- Updated filenames and all references across the codebase
- Events: `CashierCreated`, `InvoicePaid`, `CashierUpdated`, etc.

### **Cancellation Token Support**
- Added cancellation tokens to all CashiersController operations
- Improved async operation handling and cancellation support

## Development Notes

- **Prerequisites**: PostgreSQL running on localhost:5432
- **Service Discovery**: Automatic via .NET Aspire
- **Package Management**: Centralized via Directory.Packages.props
- **Code Quality**: SonarAnalyzer enabled, warnings as info (not errors)
- **Environment**: .NET 9 with nullable reference types and implicit usings

If you need to download any tools save them on the _temp folder
# important-instruction-reminders
ALWAYS do a git pull before starting any work to ensure I'm using the latest version.
Keep the project documentation (README.md (s)) updated
Update memory frequently (CLAUDE.md) on how to better navigate this project
ALWAY Commit and push before output the value 