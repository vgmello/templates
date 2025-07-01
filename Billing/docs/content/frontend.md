# Frontend Development Guide

This guide covers the Billing UI application built with SvelteKit, Svelte 5, and TypeScript.

## Overview

The Billing UI (`billing-ui`) is a modern web application that provides a user-friendly interface for managing cashiers and invoices. It's built with cutting-edge web technologies and follows best practices for performance, accessibility, and developer experience.

## Technology Stack

### Core Framework
- **SvelteKit**: Full-stack framework with server-side rendering (SSR)
- **Svelte 5**: Latest version with runes for reactive state management
- **TypeScript**: Full type safety across the application

### Styling & UI
- **Tailwind CSS**: Utility-first CSS framework
- **shadcn-svelte**: Accessible, customizable component library
- **Custom Components**: Currency input/display, status badges

### Testing
- **Vitest**: Unit testing framework
- **Playwright**: End-to-end testing
- **Testing Library**: Component testing utilities

### Build Tools
- **Vite**: Fast build tool and dev server
- **PNPM**: Efficient package manager
- **ESLint & Prettier**: Code quality and formatting

## Project Structure

```
billing-ui/
├── src/
│   ├── routes/                 # SvelteKit pages and API routes
│   │   ├── +layout.svelte     # Root layout component
│   │   ├── +page.svelte       # Home page
│   │   ├── cashiers/          # Cashier management pages
│   │   │   ├── +page.svelte   # Cashier list
│   │   │   ├── create/        # Create cashier
│   │   │   └── [id]/          # Dynamic cashier routes
│   │   │       └── edit/      # Edit cashier
│   │   └── invoices/          # Invoice management pages
│   │       ├── +page.svelte   # Invoice list
│   │       ├── create/        # Create invoice
│   │       └── [id]/          # Invoice details
│   ├── lib/
│   │   ├── api/              # API client layer
│   │   │   ├── client.ts     # Base API client
│   │   │   ├── cashiers.ts   # Cashier API methods
│   │   │   └── invoices.ts   # Invoice API methods
│   │   ├── components/       # Reusable components
│   │   │   ├── ui/          # shadcn-svelte components
│   │   │   └── InvoiceStatusBadge.svelte
│   │   ├── server/          # Server-side code
│   │   │   ├── api-client.ts # Backend API client
│   │   │   └── db/          # Database utilities
│   │   ├── types/           # TypeScript definitions
│   │   └── utils/           # Utility functions
│   ├── app.html             # HTML template
│   ├── app.css              # Global styles
│   └── hooks.server.ts      # Server hooks
├── static/                  # Static assets
├── tests/                   # Test files
└── configuration files      # Various config files
```

## Key Features

### Server-Side Rendering (SSR)
All pages are rendered on the server for optimal performance and SEO:
```typescript
// +page.server.ts
export async function load({ fetch }) {
  const invoices = await api.invoices.list({ fetch });
  return { invoices };
}
```

### Form Actions
Progressive enhancement with JavaScript-optional forms:
```typescript
// +page.server.ts
export const actions = {
  create: async ({ request, fetch }) => {
    const data = await request.formData();
    const result = await api.cashiers.create({
      name: data.get('name'),
      email: data.get('email')
    }, { fetch });
    return { success: true, cashier: result };
  }
};
```

### Type-Safe API Client
Generated types ensure type safety across the stack:
```typescript
// lib/api/invoices.ts
export async function getInvoice(
  id: string,
  options?: RequestOptions
): Promise<Invoice> {
  return client.get(`/invoices/${id}`, options);
}
```

### Currency Components
Specialized components for monetary values:
```svelte
<!-- CurrencyInput.svelte -->
<script>
  import { formatCurrency, parseCurrency } from '$lib/utils/currency';
  
  export let value = 0;
  export let currency = 'USD';
  
  $: formatted = formatCurrency(value, currency);
</script>

<input
  type="text"
  value={formatted}
  on:blur={(e) => value = parseCurrency(e.target.value)}
/>
```

### Responsive Design
Mobile-first approach with Tailwind CSS:
```svelte
<div class="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
  <!-- Responsive grid layout -->
</div>
```

## Development Workflow

### Getting Started
```bash
cd Billing/web/billing-ui
pnpm install              # Install dependencies
pnpm dev                  # Start dev server
```

### Available Scripts
```bash
pnpm dev          # Start development server (http://localhost:5173)
pnpm build        # Build for production
pnpm preview      # Preview production build
pnpm check        # TypeScript type checking
pnpm lint         # Run ESLint
pnpm format       # Format code with Prettier
pnpm test:unit    # Run unit tests
pnpm test:e2e     # Run E2E tests
```

### Environment Variables
Create a `.env` file for local development:
```env
PUBLIC_API_URL=http://localhost:8101
```

## Component Development

### Creating a New Component
1. Create component file in `src/lib/components/`
2. Export from `src/lib/components/index.ts`
3. Use TypeScript for props:

```svelte
<!-- Button.svelte -->
<script lang="ts">
  export let variant: 'primary' | 'secondary' = 'primary';
  export let disabled = false;
</script>

<button
  class="btn btn-{variant}"
  {disabled}
  on:click
>
  <slot />
</button>
```

### Using shadcn-svelte
Install new components:
```bash
npx shadcn-svelte@latest add button
```

Customize in `components.json`:
```json
{
  "style": "default",
  "tailwind": {
    "config": "tailwind.config.js",
    "css": "src/app.css"
  }
}
```

## State Management

### Svelte 5 Runes
Use runes for reactive state:
```svelte
<script>
  let count = $state(0);
  let doubled = $derived(count * 2);
  
  $effect(() => {
    console.log(`Count changed to ${count}`);
  });
</script>
```

### Context API
Share state between components:
```typescript
// context.ts
export const key = Symbol();

export interface AppContext {
  user: User;
  theme: 'light' | 'dark';
}

// +layout.svelte
setContext(key, { user, theme });

// Component.svelte
const { user, theme } = getContext<AppContext>(key);
```

## Testing

### Unit Tests
```typescript
// component.test.ts
import { render, screen } from '@testing-library/svelte';
import { expect, test } from 'vitest';
import Button from './Button.svelte';

test('renders button with text', () => {
  render(Button, { props: { children: 'Click me' } });
  expect(screen.getByText('Click me')).toBeInTheDocument();
});
```

### E2E Tests
```typescript
// invoice.spec.ts
import { test, expect } from '@playwright/test';

test('create invoice flow', async ({ page }) => {
  await page.goto('/invoices/create');
  await page.fill('input[name="amount"]', '100.00');
  await page.selectOption('select[name="currency"]', 'USD');
  await page.click('button[type="submit"]');
  
  await expect(page).toHaveURL(/\/invoices\/[a-f0-9-]+/);
});
```

## Performance Optimization

### Code Splitting
SvelteKit automatically code-splits by route:
```typescript
// Lazy load heavy components
const HeavyChart = lazy(() => import('$lib/components/HeavyChart.svelte'));
```

### Image Optimization
Use SvelteKit's image optimization:
```svelte
<enhanced:img
  src="/logo.png"
  alt="Logo"
  sizes="(max-width: 640px) 100vw, 640px"
/>
```

### Preloading
Preload data for faster navigation:
```svelte
<a href="/invoices" data-sveltekit-preload-data>
  View Invoices
</a>
```

## Best Practices

### File Naming
- Components: PascalCase (`InvoiceCard.svelte`)
- Utilities: camelCase (`formatCurrency.ts`)
- Routes: kebab-case folders

### Type Safety
- Always define prop types
- Use TypeScript for all `.ts` files
- Generate types from API schemas

### Accessibility
- Use semantic HTML
- Add ARIA labels where needed
- Test with keyboard navigation
- Ensure color contrast compliance

### Error Handling
```svelte
{#if form?.error}
  <Alert variant="error">
    {form.error.message}
  </Alert>
{/if}
```

## Deployment

### Build for Production
```bash
pnpm build
```

### Environment Configuration
Set production environment variables:
```env
PUBLIC_API_URL=https://api.billing.example.com
```

### Docker Deployment
The application includes a multi-stage Dockerfile:
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
COPY --from=builder /app/package.json ./
RUN npm ci --production
EXPOSE 3000
CMD ["node", "build"]
```

## Troubleshooting

### Common Issues

**Vite HMR not working**
- Check that ports 5173 and 24678 are not blocked
- Try clearing Vite cache: `rm -rf node_modules/.vite`

**TypeScript errors**
- Run `pnpm check` to see all errors
- Ensure `tsconfig.json` extends SvelteKit config

**Build failures**
- Clear build cache: `rm -rf .svelte-kit`
- Update dependencies: `pnpm update`

**Test failures**
- Update test snapshots: `pnpm test:unit -- -u`
- Check for timing issues in E2E tests

## Resources

- [SvelteKit Documentation](https://kit.svelte.dev)
- [Svelte 5 Runes](https://svelte.dev/docs/runes)
- [Tailwind CSS](https://tailwindcss.com)
- [shadcn-svelte](https://shadcn-svelte.com)
- [Playwright](https://playwright.dev)
- [Vitest](https://vitest.dev)