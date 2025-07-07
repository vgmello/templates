# Context Findings

**Date:** 2025-07-07
**Phase:** Context Gathering

## Current Architecture Analysis

### 1. BFF Services Layer
**Location:** `/lib/server/`
- `CashierBffService.ts` - Simple wrapper around API calls
- `InvoiceBffService.ts` - Adds client-side summary calculation

**Current Pattern:**
```typescript
// BFF Service
export const getInvoicesWithSummary = async (request) => {
  const result = await serverApiClient.get('/invoices', options);
  // Calculates summary client-side
  return { invoices, summary };
}

// Used in +page.server.ts
const result = await invoiceBffService.getInvoicesWithSummary(request);
```

### 2. State Management
**Already using Svelte 5 patterns correctly:**
- Component state: `let searchTerm = $state('')`
- Derived values: `let filtered = $derived(...)`
- Props: `let { invoice } = $props()`
- Bindable: `let { value = $bindable() } = $props()`

### 3. API Client Architecture
**Two implementations:**
1. **Server API Client** (`/lib/server/api-client.ts`)
   - Direct backend calls with auth headers
   - OpenTelemetry instrumentation
   - Used in +page.server.ts

2. **Browser API Client** (`/lib/api-client.ts`)
   - Routes through Vite proxy `/api`
   - Used in +page.svelte for client-side actions

### 4. Error Handling Patterns
- Custom error classes: `ServerApiError`, `ApiError`
- SvelteKit error() for server-side failures
- Component-level try-catch with local error state
- Form validation with field-level errors

### 5. Infrastructure Code

**Telemetry (`/lib/telemetry/`):**
- Comprehensive OpenTelemetry setup
- Browser and server instrumentation
- Automatic fetch tracking
- Context propagation

**Files needing refactoring:**
- `/lib/telemetry/browser.ts` - Could use decorator pattern
- `/lib/telemetry/server.ts` - Could be simplified
- `hooks.server.ts` - Clean but could extract telemetry logic

### 6. Component Patterns
**Well-structured components using:**
- shadcn-svelte UI library
- Tailwind CSS
- Responsive design
- Currency formatting utilities
- Proper TypeScript types

### 7. Domain Concepts Identified
- **Entities:** Invoice, Cashier, Currency
- **Value Objects:** Money, InvoiceStatus, DateRange
- **Aggregates:** InvoiceWithCashier, InvoiceSummary
- **Services:** PaymentProcessing, InvoiceGeneration

## Files to Refactor

### High Priority
1. `/lib/server/CashierBffService.ts` - Remove, move to +page.server.ts
2. `/lib/server/InvoiceBffService.ts` - Remove, move logic to domain services
3. `/lib/server/api-client.ts` - Extract to infrastructure layer
4. `/lib/api-client.ts` - Unify with server client where possible

### Medium Priority
5. `/lib/telemetry/*` - Create unified telemetry service
6. `/lib/utils/currency.ts` - Move to domain value objects
7. `hooks.server.ts` - Extract telemetry to separate module

### Low Priority (Already Good)
8. Component files - Already using Svelte 5 patterns well
9. UI components - Preserve as-is per requirements
10. Styling - Keep current Tailwind approach

## Patterns to Implement

### 1. Domain Layer
```typescript
// Domain models with Svelte 5 state
export class Invoice {
  id = $state<string>('');
  amount = $state<Money>(new Money(0, 'USD'));
  status = $state<InvoiceStatus>('draft');
  
  get isOverdue() {
    return $derived(this.status === 'pending' && isPast(this.dueDate));
  }
}
```

### 2. Application Services
```typescript
// Use cases with clear boundaries
export class InvoiceService {
  constructor(private repository: InvoiceRepository) {}
  
  async createInvoice(command: CreateInvoiceCommand) {
    // Business logic here
  }
}
```

### 3. Infrastructure Layer
```typescript
// Unified API client
export class ApiClient {
  constructor(private config: ApiConfig) {}
  
  @traced('api.call')
  async request<T>(options: RequestOptions): Promise<T> {
    // Implementation
  }
}
```

## Migration Strategy

1. **Phase 1:** Create domain models and value objects
2. **Phase 2:** Extract application services from BFF
3. **Phase 3:** Unify API clients in infrastructure
4. **Phase 4:** Refactor telemetry with decorators
5. **Phase 5:** Update +page.server.ts files
6. **Phase 6:** Add comprehensive error boundaries

## Related Features Analyzed
- Payment processing flow
- Invoice generation workflow  
- Multi-currency support
- Real-time validation
- Responsive data tables