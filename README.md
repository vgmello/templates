# .NET 9 Microservices Template

A comprehensive .NET 9 microservices template implementing Domain-Driven Design (DDD) patterns with modern cloud-native technologies. This template serves as a production-ready starting point for building distributed systems and is designed to become a dotnet template package in the future.

## 🎯 Template Vision

This project is being developed as a **future dotnet template** that developers can use to quickly scaffold robust microservices architectures. The template will provide:

- Pre-configured microservices with DDD patterns
- Production-ready infrastructure components
- Modern observability and messaging patterns
- Comprehensive testing strategies
- Database schema management with Liquibase

## 🏗️ Architecture Overview

This template implements a microservices architecture using Domain-Driven Design principles with the following core patterns:

### Service Structure Pattern

Each microservice follows a consistent layered architecture:

```
DomainName/
├── src/
│   ├── DomainName.Api/                  # REST/gRPC endpoints and controllers
│   ├── DomainName.AppHost/              # .NET Aspire orchestration host
│   ├── DomainName.BackOffice/           # Background jobs and event handlers
│   ├── DomainName.BackOffice.Orleans/   # Orleans grains (optional)
│   ├── DomainName.Contracts/            # Integration events and shared models
│   └── DomainName/                      # Core domain logic (commands, queries, entities)
├── test/
│   └── DomainName.Tests/                # Integration and architecture tests
└── infra/
  └── DomainName.Database/             # Liquibase database migrations
```

### Current Services

- **🧾 Billing Service** - Manages cashiers, invoices, and payment processing
- **📊 Accounting Service** - Handles ledgers, business day operations, and financial records
- **🔧 Platform/Operations** - Shared infrastructure, extensions, and service defaults

## 🔧 Technology Stack

### Core Framework
- **.NET 9** - Latest framework with C# 13
- **ASP.NET Core** - Web API and gRPC services
- **Minimal APIs** - Lightweight HTTP APIs

### Messaging & Communication
- **WolverineFx** - CQRS/Event Sourcing framework
- **gRPC** - High-performance inter-service communication
- **Apache Kafka** - Distributed streaming platform
- **Protocol Buffers** - Efficient serialization

### Data & Persistence
- **PostgreSQL** - Primary database engine
- **Dapper** - Lightweight ORM with custom source generators
- **Liquibase** - Database schema versioning and migration

### Orchestration & Service Discovery
- **.NET Aspire** - Cloud-native application orchestration
- **Microsoft Orleans** - Virtual actor model (for stateful services)
- **Service Discovery** - Automatic service registration and discovery

### Observability & Monitoring
- **OpenTelemetry** - Distributed tracing and metrics
- **Serilog** - Structured logging
- **Health Checks** - Service health monitoring
- **Orleans Dashboard** - Real-time insights into Orleans grains

### Testing
- **xUnit v3** - Unit and integration testing framework
- **Testcontainers** - Integration testing with real dependencies
- **NetArchTest** - Architecture compliance testing
- **NSubstitute** - Mocking framework
- **Shouldly** - Fluent assertions

### Code Quality & Analysis
- **SonarAnalyzer** - Static code analysis
- **FluentValidation** - Input validation with automatic registration

### Development Tools
- **Docker & Docker Compose** - Containerization
- **Azure Storage** - Cloud storage integration
- **HTTP Files** - API testing and documentation

## 🚀 Quick Start

Choose your preferred development approach:

## ⚡ Quick Start - Aspire (Recommended)

The fastest way to get started is using .NET Aspire orchestration for a specific domain.

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL](https://www.postgresql.org/download/) running on `localhost:5432`

### 🧾 Billing Domain
```bash
# Clone and run Billing services
git clone <repository-url>
cd templates
cd Billing/src/Billing.AppHost
dotnet run
```

### 📊 Accounting Domain  
```bash
# Clone and run Accounting services
git clone <repository-url>
cd templates
cd Accounting/src/Accounting.AppHost
dotnet run
```

**That's it!** 🎉 Aspire automatically:
- ✅ Sets up databases with Liquibase
- ✅ Starts all domain services (API, BackOffice, Orleans)
- ✅ Configures service discovery
- ✅ Provides observability dashboard

**Access Points:**
- **Billing Aspire Dashboard**: http://localhost:18100
- **Accounting Aspire Dashboard**: http://localhost:18120  
- **Operations Aspire Dashboard**: http://localhost:18140
- **Domain APIs**: Available through respective Aspire dashboards

### Port Assignment Pattern

The system uses a structured port allocation optimized for macOS compatibility:

#### **Service Port Ranges**
- **Billing**: 8100-8119 (20 ports)
- **Accounting**: 8120-8139 (20 ports)  
- **Operations**: 8140-8159 (20 ports)

#### **Port Pattern Within Each Service**
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

#### **Aspire Dashboard Ports**
- **HTTP**: Service base + 10,000 (e.g., 8100 → 18100)
- **HTTPS**: Service base + 10,010 (e.g., 8100 → 18110)

#### **Service Access Points**
- **Billing Service**: http://localhost:8101 (API), http://localhost:8119 (Docs)
- **Accounting Service**: http://localhost:8121 (API), http://localhost:8139 (Docs - reserved)
- **Operations Service**: http://localhost:8159 (Docs - reserved)
- **Shared Services**: PostgreSQL (5432), OTLP (4317/4318)

---

## 🔧 Manual Setup (Full Control)

For developers who want full control over the setup process.

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL](https://www.postgresql.org/download/) running on `localhost:5432`
- [Liquibase CLI](https://docs.liquibase.com/install/home.html) (for database setup)
- [Docker](https://docs.docker.com/get-docker/) (optional)

### 1. Clone and Build
```bash
git clone <repository-url>
cd templates
dotnet build Operations.slnx
```

### 2. Database Setup
Set up databases manually using Liquibase:

> Important: The following commands assume your running PosgresSQL DB running on port 5432, has the username `postgres` and password `password@`, adjust if needed, by providing the CLI params `--password` or `--username`

```bash
# Billing Database
cd Billing/infra/Billing.Database/
liquibase update --defaults-file liquibase.setup.properties
liquibase update --defaults-file liquibase.servicebus.properties
liquibase update

# Accounting Database
cd ../../../Accounting/infra/Accounting.Database/
liquibase update --defaults-file liquibase.setup.properties
liquibase update --defaults-file liquibase.servicebus.properties
liquibase update
```

For detailed database instructions, see [DATABASE_SETUP.md](DATABASE_SETUP.md).

### 3. Run Services

**Option A: Individual Services**
```bash
# Terminal 1 - Billing API
dotnet run --project Billing/src/Billing.Api

# Terminal 2 - Billing BackOffice
dotnet run --project Billing/src/Billing.BackOffice

# Terminal 3 - Accounting API
dotnet run --project Accounting/src/Accounting.Api

# Terminal 4 - Accounting BackOffice
dotnet run --project Accounting/src/Accounting.BackOffice
```

**Option B: Docker Compose**
```bash
docker compose up --build
```

**Option C: Individual Docker Builds**
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

### 4. Verify Manual Setup
- **Billing API**: http://localhost:8101/scalar
- **Accounting API**: http://localhost:8121/scalar
- **Health Checks**: http://localhost:8101/health, http://localhost:8121/health

---

## 🧪 Quick Test

Test the APIs with the included HTTP files:

```bash
# Install REST Client extension in VS Code, then open:
# .http/Billing.Api.http
# .http/Accounting.Api.http
```

Or use curl:
```bash
# Create a cashier
curl -X POST http://localhost:8101/cashiers \
  -H "Content-Type: application/json" \
  -d '{"name": "Test Cashier", "currencyCode": "USD"}'

# Create a ledger
curl -X POST http://localhost:8121/ledgers \
  -H "Content-Type: application/json" \
  -d '{"name": "Test Ledger", "type": "Asset"}'
```

## 📋 Development Commands

### Building and Testing
```bash
# Build entire solution
dotnet build Operations.slnx

# Run all tests
dotnet test

# Run specific service tests
dotnet test Billing/test/Billing.Tests
dotnet test Accounting/test/Accounting.Tests
```

### Docker Commands
```bash
# Build all service images
docker build -t billing-api -f Billing/src/Billing.Api/Dockerfile .
docker build -t billing-backoffice -f Billing/src/Billing.BackOffice/Dockerfile .
docker build -t accounting-api -f Accounting/src/Accounting.Api/Dockerfile .
docker build -t accounting-backoffice -f Accounting/src/Accounting.BackOffice/Dockerfile .

# Run with Docker Compose (full stack)
docker-compose up --build

# Run individual containers
docker run -p 8101:8080 --name billing-api billing-api
docker run -p 8103:8080 --name billing-backoffice billing-backoffice
docker run -p 8121:8080 --name accounting-api accounting-api
docker run -p 8123:8080 --name accounting-backoffice accounting-backoffice
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

## 🧩 Key Features

### 🔨 Custom Source Generators

The template includes powerful source generators for reducing boilerplate:

#### DbCommand Source Generator
Automatically generates Dapper command handlers and parameter mappers:

```csharp
[DbCommand(sp: "create_cashier", nonQuery: true)]
public partial record CreateCashierCommand(string Name, decimal Balance) : ICommand<int>;

// Generates: DB access methods that executes the stored procedure create_cashier and return the affected records (nonQuery)
```

For detailed documentation, see [Platform/src/Operations.Extensions.SourceGenerators/README.md](Platform/src/Operations.Extensions.SourceGenerators/README.md).

### 🔄 CQRS with WolverineFx

Command and Query separation with automatic handler discovery:

```csharp
// Command
public record CreateLedgerCommand(string Name, LedgerType Type) : ICommand<Guid>;

// Query  
public record GetLedgerQuery(Guid Id) : IQuery<Ledger>;

// Handlers auto-discovered and registered
public class CreateLedgerHandler
{
    public async Task<Guid> Handle(CreateLedgerCommand command, CancellationToken ct)
    {
        // Implementation
    }
}
```

### 📡 gRPC Service Communication

Type-safe inter-service communication with Protocol Buffers:

```protobuf
service Ledgers {
  rpc GetLedger(GetLedgerRequest) returns (GetLedgerResponse);
  rpc CreateLedger(CreateLedgerRequest) returns (CreateLedgerResponse);
}
```

### 🔍 Comprehensive Testing

Multiple testing layers with real infrastructure:

```csharp
[Fact]
public async Task CreateCashier_ShouldReturnSuccess()
{
    // Integration test with Testcontainers PostgreSQL
    await using var app = new BillingApiWebAppFactory();
    var client = app.CreateClient();
    
    var response = await client.PostAsJsonAsync("/cashiers", new CreateCashierRequest(...));
    
    response.StatusCode.ShouldBe(HttpStatusCode.Created);
}
```

### 📊 Built-in Observability

Production-ready monitoring and tracing:

- **Distributed Tracing** - OpenTelemetry with automatic instrumentation
- **Structured Logging** - Serilog with correlation IDs
- **Health Checks** - Endpoint monitoring with custom checks
- **Metrics** - Custom metrics with OpenTelemetry

## 📁 Project Structure

```
/
├── 📋 Operations.slnx              # Solution file with organized folders
├── 🐳 compose.yaml                 # Docker Compose services
├── 📦 Directory.Packages.props     # Centralized NuGet package management
├── 🏗️ Directory.Build.props        # Shared MSBuild properties
│
├── 🧾 Billing/                     # Billing microservice
│   ├── docs/                       # Project documentation, including code docs
│   ├── src/                        # Source code
│   ├── test/                       # Tests
│   ├── web/                        # UI project
│   └── infra/                      # Infrastructure (database)
│
├── 📊 Accounting/                  # Accounting microservice
│   ├── src/                        # Source code  
│   ├── test/                       # Tests
│   └── infra/                      # Infrastructure (database)
│
└── 🔧 Platform/                    # Shared platform components
    ├── src/
    │   ├── Operations.Extensions/           # Custom extensions and utilities
    │   ├── Operations.Extensions.Abstractions/  # Interfaces and attributes
    │   ├── Operations.Extensions.SourceGenerators/  # Code generators
    │   ├── Operations.ServiceDefaults/      # Shared service configuration
    │   ├── Operations.ServiceDefaults.Api/  # API-specific defaults
    │   └── Operations.AppHost/              # Aspire orchestration
    └── test/                               # Platform tests
```

## 🔧 Configuration

### Global Settings

The template uses centralized configuration management:

- **Directory.Build.props** - Shared MSBuild properties and package references
- **Directory.Packages.props** - Centralized package version management  
- **Operations.ruleset** - Code analysis rules
- **.editorconfig** - Code style enforcement

### Environment Configuration

Services support multiple configuration sources:
- **appsettings.json** - Base configuration
- **appsettings.Development.json** - Development overrides
- **Environment Variables** - Container/cloud deployment
- **User Secrets** - Local development secrets

### Source Generator Configuration

Configure source generators globally via MSBuild properties:

```xml
<PropertyGroup>
  <!-- If using postgres -->
  <DbCommandDefaultParamCase>SnakeCase</DbCommandDefaultParamCase>
</PropertyGroup>
```

## 🧪 Testing Strategy

### Test Categories

1. **Unit Tests** - Fast, isolated component tests
2. **Integration Tests** - Service-level tests with real dependencies
3. **Architecture Tests** - Enforce architectural constraints

### Test Infrastructure

- **Testcontainers** - Real PostgreSQL instances for integration tests
- **WebApplicationFactory** - In-memory test servers
- **Custom Test Fixtures** - Shared test infrastructure
- **Test Data Builders** - Consistent test data creation

## 🚀 Deployment

### Container Support

All services include optimized Dockerfiles:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
# Multi-stage builds for optimal image size
```

### Database Migrations

Liquibase handles database schema evolution:

- **Version Control** - All schema changes tracked in Git
- **Rollback Support** - Safe rollback to previous versions
- **Environment Promotion** - Consistent schema across environments

## 🔮 Future Template Features

When this becomes a dotnet template, it will support:

### Template Parameters
```bash
dotnet new microservice-template \
  --name "Billing" \
  --database "pgsql" \
  --messaging "kafka" \
```

### Customization Options
- **Service Selection** - Choose which services to include
- **Technology Choices** - Alternative databases, messaging systems
- **Authentication** - JWT, OAuth2, Azure AD integration

## 🤝 Contributing

This template is designed to incorporate best practices and patterns. Contributions are welcome for:

- New microservice patterns
- Additional source generators
- Enhanced testing strategies
- Cloud deployment templates
- Documentation improvements

## 📚 Additional Resources

- **[CLAUDE.md](CLAUDE.md)** - AI assistant development guidance
- **[DATABASE_SETUP.md](DATABASE_SETUP.md)** - Detailed database setup instructions
- **[Source Generator Documentation](Platform/src/Operations.Extensions.SourceGenerators/README.md)** - DbCommand generator guide