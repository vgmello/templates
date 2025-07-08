# Billing UI

Modern SvelteKit web application for the Billing service, providing a responsive interface for managing cashiers and invoices with domain-driven design principles.

## Overview

The Billing UI is a full-stack SvelteKit application that implements:

- **Domain-Driven Architecture**: Feature-based organization with command/query patterns
- **Server-side rendering** with client-side hydration using SvelteKit
- **Type-safe API integration** with comprehensive error handling
- **Reactive domain models** using Svelte 5's `$state` and `$derived`
- **Responsive design** using Tailwind CSS and shadcn-svelte components
- **Progressive enhancement** with form actions and real-time validation
- **OpenTelemetry integration** for comprehensive observability

## Tech Stack

- **Framework**: SvelteKit with Svelte 5
- **Language**: TypeScript with strict typing
- **Architecture**: Domain-driven design with CQRS patterns
- **Styling**: Tailwind CSS + shadcn-svelte components
- **Icons**: Lucide Svelte
- **Observability**: OpenTelemetry with tracing
- **Testing**: Vitest (unit) + Playwright (E2E)

## Getting Started

### Prerequisites

- Node.js 18+ and pnpm (or npm)
- Billing API running on http://localhost:8101
- PostgreSQL for session storage (optional, uses API's database)

### Installation

```bash
# Install dependencies
pnpm install

# Start development server
pnpm dev

# The UI will be available at http://localhost:5173
```

### Development Commands

```bash
# Development server with hot reload
pnpm dev

# Type checking
pnpm check
pnpm check:watch  # Watch mode

# Linting and formatting
pnpm lint         # Check code style
pnpm format       # Auto-format code

# Testing
pnpm test:unit    # Unit tests with Vitest
pnpm test:e2e     # E2E tests with Playwright
pnpm test         # Run all tests

# Building
pnpm build        # Production build
pnpm preview      # Preview production build

# Database (for auth/sessions)
pnpm db:push      # Push schema changes
pnpm db:migrate   # Run migrations
pnpm db:studio    # Open Drizzle Studio
```

## Project Structure

The application follows a domain-driven design with feature-based organization:

```
billing-ui/src/
├── routes/                    # SvelteKit routes (pages)
│   ├── +layout.svelte        # Root layout with navigation
│   ├── +page.svelte          # Dashboard home
│   ├── cashiers/             # Cashier feature routes
│   └── invoices/             # Invoice feature routes
├── lib/
│   ├── cashiers/             # Cashier domain feature
│   │   ├── actions/          # Commands (CreateCashierCommand)
│   │   ├── components/       # Domain-specific components
│   │   ├── models/           # Domain models (Cashier.ts)
│   │   ├── queries/          # Queries (GetCashiersQuery)
│   │   ├── validators/       # Business validation
│   │   └── CashiersApi.ts    # API integration
│   ├── invoices/             # Invoice domain feature
│   │   ├── actions/          # Commands (CreateInvoiceCommand)
│   │   ├── components/       # Domain-specific components
│   │   ├── models/           # Domain models (Invoice.ts)
│   │   ├── queries/          # Queries (GetInvoicesQuery)
│   │   └── InvoiceApi.ts     # API integration
│   ├── core/                 # Shared domain values
│   │   ├── values/           # Value objects (Money, Currency)
│   │   └── enums/            # Domain enumerations
│   ├── infrastructure/       # Cross-cutting concerns
│   │   ├── api/              # HTTP client with telemetry
│   │   ├── error/            # Error handling & notifications
│   │   └── telemetry/        # OpenTelemetry setup
│   ├── server/               # Server-side utilities
│   └── ui/                   # shadcn-svelte components
├── tests/                    # Test files
├── static/                   # Static assets
└── *.config.js              # Configuration files
```

## Features

### Cashier Management

- List cashiers with pagination
- Create new cashiers with validation
- Edit cashier information
- Delete cashiers (with confirmation)
- Multi-currency support

### Invoice Management

- List invoices with filtering by status
- Create invoices linked to cashiers
- View invoice details
- Cancel unpaid invoices
- Mark invoices as paid
- Copy invoice reference numbers

### UI Components

The application uses shadcn-svelte components for consistent design:

- **Badge**: Status indicators
- **Button**: Interactive elements
- **Card**: Content containers
- **Dialog**: Modal interactions
- **Input**: Form fields
- **Table**: Data display
- **CurrencyInput**: Formatted currency entry
- **CurrencyDisplay**: Formatted currency display

### API Integration

The UI uses a command/query pattern with domain-specific APIs:

```typescript
// Example: Query pattern
import { GetCashiersQuery } from '$lib/cashiers/queries/GetCashiersQuery';

const query = new GetCashiersQuery({ limit: 10, offset: 0 });
const cashiers = await query.execute();

// Example: Command pattern
import { CreateInvoiceCommand } from '$lib/invoices/actions/CreateInvoiceCommand';

const command = new CreateInvoiceCommand({
	name: 'Invoice #123',
	amount: 100.5,
	currency: 'USD',
	dueDate: new Date(),
	cashierId: '...'
});
const invoice = await command.execute();
```

### Domain Models

Reactive domain models using Svelte 5's runes:

```typescript
// Reactive domain model
export class Cashier {
	id = $state<string>('');
	name = $state<string>('');
	email = $state<string>('');

	// Derived computed properties
	displayName = $derived(this.name || this.email || 'Unknown Cashier');
	isValid = $derived(this.name.length > 0 && this.email.length > 0);
}
```

### Form Handling

Forms use SvelteKit's progressive enhancement:

```svelte
<form method="POST" use:enhance>
	<!-- Form fields -->
</form>
```

Server-side actions handle validation and API calls, providing a seamless experience even without JavaScript.

## Configuration

### Environment Variables

Create a `.env` file for local development:

```bash
# API Configuration
PUBLIC_API_URL=http://localhost:8101

# Database (for sessions)
DATABASE_URL=postgres://postgres:password@@localhost:54320/billing

# Session Secret
SESSION_SECRET=your-secret-key-here
```

### API Endpoints

The UI expects the Billing API to be available at:

- Development: `http://localhost:8101`
- Production: Configure via `PUBLIC_API_URL`

## Testing

### Unit Tests

Unit tests use Vitest and test individual components:

```bash
pnpm test:unit
```

### E2E Tests

End-to-end tests use Playwright to test full user workflows:

```bash
# Run tests
pnpm test:e2e

# Run in UI mode
pnpm test:e2e -- --ui

# Run specific test
pnpm test:e2e demo.test.ts
```

## Deployment

### Building for Production

```bash
# Build the application
pnpm build

# Preview the build
pnpm preview
```

### Docker Support

The UI can be containerized and deployed with the Billing service:

```dockerfile
FROM node:20-alpine AS builder
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM node:20-alpine
WORKDIR /app
COPY --from=builder /app/build ./build
COPY --from=builder /app/package*.json ./
RUN npm ci --production
EXPOSE 3000
CMD ["node", "build"]
```

## Architecture Philosophy: Minimal Ceremony UI

### Design Principles

Following the backend's **minimal ceremony** approach, the UI architecture mirrors real-world operations while avoiding unnecessary abstractions:

1. **No Domain Objects**: The UI doesn't need domain models with behavior - all business logic belongs in the backend
2. **API Types for Display**: Use API response types directly for displaying data
3. **Form State for Input**: Simple reactive classes for collecting user input
4. **Commands/Queries as Coordinators**: Thin wrappers that coordinate API calls, not business logic
5. **Real-World Operations**: Each operation reflects actual billing department work

### Architecture Components

#### 1. API Types (Data Display)

```typescript
// Use backend API types directly - no transformation needed
export interface GetCashiersResult {
	cashierId: string;
	name: string;
	email: string;
	createdDateUtc?: string;
}
```

#### 2. Form State (User Input)

```typescript
// Simple reactive classes for form handling
export class CreateCashierForm {
	name = $state('');
	email = $state('');

	isValid = $derived(this.name.trim() !== '' && this.email.trim() !== '');

	toRequest(): CreateCashierRequest {
		return { name: this.name.trim(), email: this.email.trim() };
	}
}
```

#### 3. Commands (API Coordination)

```typescript
// Thin wrappers that coordinate API calls only
export class CreateCashierCommand {
	constructor(private readonly request: CreateCashierRequest) {}

	async execute(): Promise<CashierDTO> {
		if (!this.request.name?.trim() || !this.request.email?.trim()) {
			throw new Error('Name and email are required');
		}
		return await cashierApi.createCashier(this.request);
	}
}
```

#### 4. Queries (Data Retrieval)

```typescript
// Simple coordinators for data fetching
export class GetCashiersQuery {
	constructor(private readonly query?: GetCashiersQueryParams) {}

	async execute(): Promise<GetCashiersResult[]> {
		return await cashierApi.getCashiers(this.query);
	}
}
```

### Why This Approach?

1. **Simplicity**: No complex domain models, value objects, or business logic
2. **Clarity**: Each file has a single, clear responsibility
3. **Maintainability**: Changes to business logic happen in the backend only
4. **Performance**: No unnecessary object creation or transformation
5. **Real-World Alignment**: Operations mirror actual billing department workflows

### What We Removed

- ❌ Complex domain models with behavior (`Cashier.activate()`, `Invoice.validate()`)
- ❌ UI-specific domain logic and validation
- ❌ Value objects in UI layer
- ❌ Business rule enforcement in frontend
- ❌ Smart objects that can change themselves

### What We Kept

- ✅ Command/Query pattern for organization
- ✅ API types for data display
- ✅ Reactive form state with Svelte 5
- ✅ Simple validation in forms
- ✅ OpenTelemetry integration
- ✅ Error handling and notifications

This results in a UI that's purely concerned with presentation and user interaction, while all business logic remains where it belongs - in the backend service.

## OpenTelemetry Integration

### Packages Used

**Core**: `@opentelemetry/api`, `@opentelemetry/resources`, `@opentelemetry/semantic-conventions`
**Web**: `@opentelemetry/sdk-trace-web`, `@opentelemetry/instrumentation-fetch`
**Node**: `@opentelemetry/sdk-node`, `@opentelemetry/instrumentation-http`
**Export**: `@opentelemetry/exporter-trace-otlp-http`

### Implementation

- Automatic HTTP request tracing
- Custom spans for domain operations
- Server-side and client-side instrumentation
- Currently using mock mode due to build complexity

## Development Tips

1. **Domain Modeling**: Keep domain logic separate from UI concerns
2. **Type Safety**: Use strict TypeScript with proper layer separation
3. **Component Architecture**: Leverage shadcn-svelte for consistency
4. **Progressive Enhancement**: Ensure core functionality works without JS
5. **Error Boundaries**: Implement feature-specific error handling
6. **Performance**: Use SvelteKit's preloading and code splitting
7. **Testing**: Test domain logic separately from UI components

### Backend execution

```bash
# From the repo root folder
docker compose -f Billing/compose.yml --profile api up -d
```

## Contributing

1. Follow the existing code style (Prettier configuration)
2. Write tests for new features
3. Update types when API changes
4. Ensure accessibility standards are met
5. Test on mobile devices for responsive design
