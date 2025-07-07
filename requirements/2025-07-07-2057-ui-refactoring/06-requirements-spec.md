# Billing UI Refactoring Requirements Specification

**Date:** 2025-07-07
**Status:** Complete

## Executive Summary

This specification defines the requirements for refactoring the Billing UI application to achieve professional-grade code quality while maintaining the existing user interface and functionality. The refactoring focuses on removing the BFF service layer, implementing proper domain-driven design with Svelte 5's class-based state management, and improving infrastructure code organization.

## Problem Statement

The current Billing UI codebase lacks the professional structure and organization found in the Billing.Backend. Key issues include:
- Thin BFF service layer that adds minimal value
- Scattered business logic without clear domain boundaries
- Duplicated API client implementations
- Infrastructure code (telemetry) mixed with application logic
- Missing error boundaries and recovery mechanisms

## Solution Overview

Refactor the Billing UI to implement:
1. **Domain-oriented architecture** using Svelte 5's class-based state management
2. **Direct API integration** in +page.server.ts files (removing BFF services)
3. **Unified infrastructure layer** with clean separation of concerns
4. **Comprehensive error handling** with proper error boundaries
5. **Environment-based configuration** for deployment flexibility

## Functional Requirements

### FR1: Preserve User Experience
- All existing UI components must remain visually identical
- Current user workflows must continue to function without changes
- Response times should remain the same or improve
- Mobile responsiveness must be maintained

### FR2: API Integration
- Remove BFF service layer (`CashierBffService.ts`, `InvoiceBffService.ts`)
- Move API calls directly to +page.server.ts load and action functions
- Maintain current API contract compatibility
- Preserve existing error handling behavior

### FR3: State Management
- Implement domain models using Svelte 5 class syntax with `$state` and `$derived`
- Keep state management server-side in +page.server.ts
- Maintain current reactive UI behaviors
- Preserve form state handling patterns

### FR4: Error Handling
- Implement comprehensive error boundaries for all user actions
- Provide user-friendly error messages
- Add recovery mechanisms where appropriate
- Log errors to telemetry system

## Technical Requirements

### TR1: Architecture Structure
```
billing-ui/
├── src/
│   ├── lib/
│   │   ├── domain/           # Domain models and business logic
│   │   │   ├── models/       # Invoice, Cashier classes with $state
│   │   │   ├── values/       # Money, Currency, Status value objects
│   │   │   └── services/     # Business logic services
│   │   ├── infrastructure/   # Technical concerns
│   │   │   ├── api/          # Unified API client
│   │   │   ├── telemetry/    # OpenTelemetry setup
│   │   │   └── config/       # Environment configuration
│   │   ├── components/       # UI components (preserve as-is)
│   │   └── utils/           # Utilities and helpers
│   └── routes/              # SvelteKit routes
│       ├── invoices/
│       │   ├── +page.server.ts  # Direct API calls
│       │   └── +page.svelte      # UI with domain models
│       └── cashiers/
│           ├── +page.server.ts  # Direct API calls
│           └── +page.svelte      # UI with domain models
```

### TR2: Domain Model Implementation
```typescript
// Example: src/lib/domain/models/Invoice.ts
export class Invoice {
  id = $state<string>('');
  number = $state<string>('');
  amount = $state<Money>(new Money(0, 'USD'));
  status = $state<InvoiceStatus>('draft');
  dueDate = $state<Date>(new Date());
  cashierId = $state<string | null>(null);
  
  isOverdue = $derived(
    this.status === 'pending' && this.dueDate < new Date()
  );
  
  canBeCancelled = $derived(
    this.status === 'draft' || this.status === 'pending'
  );
  
  cancel() {
    if (!this.canBeCancelled) {
      throw new Error('Invoice cannot be cancelled');
    }
    this.status = 'cancelled';
  }
}
```

### TR3: API Client Simplification
- Merge browser and server API clients where possible
- Use environment variables for API endpoints
- Implement consistent error handling
- Remove BFF service abstractions

### TR4: Infrastructure Improvements
- Extract telemetry setup to dedicated service
- Use environment variables for configuration:
  - `OTEL_EXPORTER_OTLP_ENDPOINT`
  - `OTEL_SERVICE_NAME`
  - `API_BASE_URL`
- Create telemetry decorators for cleaner instrumentation

### TR5: +page.server.ts Pattern
```typescript
// Direct API calls without BFF layer
export async function load({ url, locals }) {
  const searchParams = // ... extract params
  
  try {
    const response = await apiClient.get('/invoices', {
      params: searchParams,
      headers: getAuthHeaders(locals)
    });
    
    // Calculate summary server-side if needed
    const summary = calculateInvoiceSummary(response.data);
    
    return {
      invoices: response.data,
      summary
    };
  } catch (error) {
    throw error(500, 'Failed to load invoices');
  }
}
```

## Implementation Guidelines

### 1. Preserve Existing Patterns
- Keep using shadcn-svelte components
- Maintain Tailwind CSS styling
- Preserve current form handling with enhance
- Keep existing route structure

### 2. Simplicity First
- Direct API calls (no repository pattern)
- Server-side validation only
- No optimistic updates
- Minimal abstraction layers

### 3. Code Organization
- Single responsibility classes
- High cohesion within modules
- Clear separation of concerns
- Domain logic separate from infrastructure

### 4. Testing Approach
- Unit tests for domain models
- Integration tests for API calls
- E2E tests remain unchanged
- Maintain existing test coverage

## Migration Plan

### Phase 1: Domain Layer (Week 1)
- Create domain models with Svelte 5 state
- Implement value objects
- Add domain services for business logic

### Phase 2: Infrastructure Layer (Week 2)
- Unify API clients
- Extract telemetry service
- Add environment configuration

### Phase 3: Remove BFF Services (Week 3)
- Migrate API calls to +page.server.ts
- Remove BFF service files
- Update imports and dependencies

### Phase 4: Error Handling (Week 4)
- Implement error boundaries
- Add recovery mechanisms
- Improve error messages

### Phase 5: Testing & Documentation (Week 5)
- Update tests for new structure
- Document architectural decisions
- Update README with new patterns

## Acceptance Criteria

1. **All existing functionality works identically** to current implementation
2. **BFF services are completely removed** from the codebase
3. **Domain models use Svelte 5 class-based state** management
4. **API calls happen directly in +page.server.ts** files
5. **Telemetry configuration uses environment variables**
6. **Error handling provides clear user feedback**
7. **All tests pass** with no reduction in coverage
8. **Code follows single responsibility** and high cohesion principles
9. **No visual changes** to the UI
10. **Performance remains the same or improves**

## Assumptions

1. The existing API contracts will not change during refactoring
2. Svelte 5 features are stable and can be relied upon
3. Current shadcn-svelte components meet all UI needs
4. Server-side rendering remains the primary rendering strategy
5. No new external dependencies are required

## Out of Scope

1. Visual redesign or UI/UX improvements
2. New features or functionality
3. Backend API modifications
4. Database schema changes
5. Authentication/authorization changes
6. Performance optimizations beyond code organization

## Success Metrics

- Code maintainability score improves (measured by SonarQube or similar)
- Developer onboarding time reduces by 50%
- Bug fix time reduces by 30%
- No increase in bundle size
- No degradation in page load times
- 100% feature parity with current implementation