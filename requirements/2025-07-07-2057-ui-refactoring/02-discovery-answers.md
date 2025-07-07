# Discovery Answers

**Date:** 2025-07-07
**Phase:** Discovery

## Q1: Should the refactored UI maintain backward compatibility with existing API contracts?
**Answer:** No, but the look and feel should remain the same
**Implications:** We can redesign internal API client structure while preserving the UI appearance

## Q2: Will the refactoring include migrating to a state management solution (like stores or state machines)?
**Answer:** Yes, using the Svelte 5 approach with classes
**Example provided:**
```typescript
export class Counter {
  count = $state(0)
  // you can also derive values
  double = $derived(this.count * 2)

  increment = () => this.count++
}
```
**Implications:** We'll implement domain-oriented state classes using Svelte 5's reactive primitives

## Q3: Should the BFF services be completely removed in favor of direct API calls from the frontend?
**Answer:** No BFF services, but use REST API client with SvelteKit server pages (+page.server.ts) as the BFF
**Implications:** Remove current BFF service layer, implement clean API client, use load functions for server-side data fetching

## Q4: Will the refactoring need to preserve all existing UI components and their visual appearance?
**Answer:** Yes
**Implications:** Focus on code structure and organization, not visual changes

## Q5: Should the refactored code include comprehensive error boundaries and recovery mechanisms?
**Answer:** Yes
**Implications:** Implement proper error handling, user-friendly error states, and recovery flows