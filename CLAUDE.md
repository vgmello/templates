# CLAUDE.md

## Billing Service

### Quick Start

#### Using .NET Aspire (Recommended)
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
- **Documentation**: http://localhost:8119

#### Using Docker Compose
```bash
# Backend services only
docker compose -f Billing/compose.yml --profile api up -d

# Backend + documentation
docker compose -f Billing/compose.yml --profile api --profile docs up -d

# All services (including Aspire dashboard)
docker compose -f Billing/compose.yml --profile api --profile backoffice --profile aspire up -d
```

### Architecture Overview

#### Backend Services
- **Billing.Api** - REST + gRPC API (ports 8101/8102)
  - Cashier management endpoints
  - Invoice processing endpoints
  - Health checks and OpenAPI documentation
- **Billing.BackOffice** - Event-driven background processing (port 8103)
  - Wolverine-based message handling
  - Integration event processing
- **Billing.BackOffice.Orleans** - Stateful invoice processing (port 8104)
  - Orleans actors with 3-replica clustering
  - Azure Storage persistence
- **Billing** (Core) - Domain logic with DDD patterns
  - Commands, queries, entities
  - FluentValidation
  - Source-generated DB commands

#### Frontend
- **billing-ui** - SvelteKit application (port 8105)
  - Svelte 5 with TypeScript
  - Tailwind CSS + shadcn-svelte components
  - Server-side rendering with client hydration

### API Endpoints

#### REST API

**Cashiers**
- `GET /cashiers` - List cashiers (paginated)
- `GET /cashiers/{id}` - Get specific cashier
- `POST /cashiers` - Create cashier
- `PUT /cashiers/{id}` - Update cashier
- `DELETE /cashiers/{id}` - Delete cashier

**Invoices**
- `GET /invoices` - List invoices (filtered, paginated)
- `GET /invoices/{id}` - Get specific invoice
- `POST /invoices` - Create invoice
- `PUT /invoices/{id}/cancel` - Cancel invoice
- `PUT /invoices/{id}/mark-paid` - Mark invoice as paid
- `POST /invoices/{id}/simulate-payment` - Simulate payment (testing)

#### gRPC Services
- `cashiers.CashiersService` - Full cashier CRUD
- `invoices.InvoicesService` - Full invoice management

### Frontend Development

#### Setup
```bash
cd Billing/web/billing-ui
pnpm install  # or npm install
```

#### Development Commands
```bash
pnpm dev          # Start dev server (http://localhost:5173)
pnpm build        # Build for production
pnpm preview      # Preview production build
pnpm check        # TypeScript type checking
pnpm lint         # ESLint + Prettier check
pnpm format       # Auto-format code
pnpm test:unit    # Run unit tests with Vitest
pnpm test:e2e     # Run E2E tests with Playwright
```

#### Key Features
- Server-side rendering with form actions
- Type-safe API client with generated types
- Responsive design with mobile support
- Currency formatting and input components
- Real-time validation and error handling

### Database

#### Structure
- **billing** database - Main application data
  - `cashiers` - Cashier information
  - `cashier_currencies` - Multi-currency support
  - `invoices` - Invoice records
- **service_bus** database - Wolverine messaging
- Managed by Liquibase migrations

#### Connection String
```
Host=localhost;Port=54320;Database=billing;Username=postgres;Password=password@
```

### Testing

#### Backend Tests
```bash
# All tests
dotnet test

# Specific project
dotnet test Billing/test/Billing.Tests

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

Test Categories:
- **Unit** - Isolated component tests with mocks
- **Integration** - Real PostgreSQL via Testcontainers
- **Architecture** - NetArchTest rule enforcement

#### Frontend Tests
```bash
cd Billing/web/billing-ui

# Unit tests (Vitest)
pnpm test:unit

# E2E tests (Playwright)
pnpm test:e2e

# All tests
pnpm test
```

### Integration Events

**Published**
- `CashierCreated`, `CashierUpdated`, `CashierDeleted`
- `InvoiceCreated`, `InvoiceCancelled`, `InvoicePaid`
- `PaymentReceived`

**Consumed**
- `BusinessDayEndedEvent` (from Accounting service)

### Development Notes

- **Package Management**: Centralized via Directory.Packages.props
- **Code Quality**: SonarAnalyzer enabled, warnings as info (not errors)
- **Environment**: .NET 9 with nullable reference types and implicit usings
- **Source Generation**: Custom generators reduce boilerplate for DB commands
- **Port Consistency**: Infrastructure uses persistent containers with fixed ports

### Troubleshooting

#### Common Issues
1. **Database connection failed**
   - Ensure PostgreSQL is running on port 54320
   - Check credentials: postgres/password@

2. **Port conflicts**
   - Check if ports 8101-8105 are available
   - Use `netstat -tulpn | grep <port>` to verify

3. **Test failures**
   - Docker must be running for Testcontainers
   - Run database migrations first

4. **Frontend build errors**
   - Clear node_modules and reinstall
   - Check Node.js version (18+ required)

If you need to download any tools save them on the _temp folder

# Important-instruction-reminders
ALWAYS do a git pull before starting any work to ensure I'm using the latest version.
Keep the project documentation (README.md (s)) updated
Update memory frequently (CLAUDE.md) on how to better navigate this project
ALWAY Commit and push before output the value