# Billing UI

Modern SvelteKit web application for the Billing service, providing a responsive interface for managing cashiers and invoices.

## Overview

The Billing UI is a full-stack SvelteKit application that provides:

- Server-side rendering with client-side hydration
- Type-safe API integration with the Billing backend
- Responsive design using Tailwind CSS and shadcn-svelte components
- Form handling with progressive enhancement
- Real-time validation and error handling

## Tech Stack

- **Framework**: SvelteKit with Svelte 5
- **Language**: TypeScript
- **Styling**: Tailwind CSS + shadcn-svelte components
- **Icons**: Lucide Svelte
- **Testing**: Vitest (unit) + Playwright (E2E)
- **Database**: PostgreSQL with Drizzle ORM (for auth/session management)

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

```
billing-ui/
├── src/
│   ├── routes/               # SvelteKit routes (pages)
│   │   ├── +layout.svelte   # Root layout
│   │   ├── +page.svelte     # Home/dashboard
│   │   ├── cashiers/        # Cashier management
│   │   │   ├── +page.svelte # List view
│   │   │   ├── create/      # Create form
│   │   │   └── [id]/        # Detail/edit views
│   │   └── invoices/        # Invoice management
│   │       ├── +page.svelte # List view
│   │       ├── create/      # Create form
│   │       └── [id]/        # Detail view
│   ├── lib/
│   │   ├── api/             # API client layer
│   │   │   ├── client.ts    # Base HTTP client
│   │   │   ├── cashiers.ts  # Cashier endpoints
│   │   │   └── invoices.ts  # Invoice endpoints
│   │   ├── components/      # Reusable components
│   │   │   ├── ui/          # shadcn-svelte components
│   │   │   └── *.svelte     # Custom components
│   │   ├── server/          # Server-side utilities
│   │   │   ├── auth.ts      # Authentication
│   │   │   └── db/          # Database schema
│   │   ├── types/           # TypeScript types
│   │   └── utils/           # Utility functions
│   └── app.html             # HTML template
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

The UI integrates with the Billing API through a type-safe client:

```typescript
// Example: Fetching cashiers
import { getCashiers } from '$lib/api/cashiers';

const cashiers = await getCashiers({
	limit: 10,
	offset: 0
});

// Example: Creating an invoice
import { createInvoice } from '$lib/api/invoices';

const invoice = await createInvoice({
	name: 'Invoice #123',
	amount: 100.5,
	currency: 'USD',
	dueDate: new Date(),
	cashierId: '...'
});
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

### Integration with .NET Aspire

When running via .NET Aspire, the UI is automatically configured with:

- Service discovery for API endpoints
- Health checks
- Distributed tracing
- Centralized logging

## Troubleshooting

### Common Issues

1. **API Connection Failed**
    - Verify Billing API is running on port 8101
    - Check `PUBLIC_API_URL` configuration
    - Ensure CORS is configured if on different ports

2. **Type Errors**
    - Run `pnpm check` to identify issues
    - Ensure API types match expected interfaces
    - Regenerate types if API changed

3. **Build Failures**
    - Clear `node_modules` and `.svelte-kit`
    - Run `pnpm install` fresh
    - Check Node.js version (18+ required)

4. **Test Failures**
    - Ensure API is running for E2E tests
    - Check test database is clean
    - Review Playwright trace on failure

## OTEL

### Core Packages

@opentelemetry/api
@opentelemetry/api-logs
@opentelemetry/exporter-trace-otlp-http
@opentelemetry/resources
@opentelemetry/instrumentation
@opentelemetry/semantic-conventions

### Web SDK

@opentelemetry/sdk-trace-web
@opentelemetry/instrumentation-fetch
@opentelemetry/instrumentation-document-load

### Node SDK

@opentelemetry/sdk-node
@opentelemetry/sdk-logs
@opentelemetry/sdk-trace-node
@opentelemetry/instrumentation-http

## Development Tips

1. **Type Safety**: Leverage TypeScript for API integration
2. **Component Reuse**: Use shadcn-svelte components consistently
3. **Progressive Enhancement**: Ensure forms work without JavaScript
4. **Accessibility**: Test with keyboard navigation and screen readers
5. **Performance**: Use SvelteKit's preloading and code splitting

## Contributing

1. Follow the existing code style (Prettier configuration)
2. Write tests for new features
3. Update types when API changes
4. Ensure accessibility standards are met
5. Test on mobile devices for responsive design
