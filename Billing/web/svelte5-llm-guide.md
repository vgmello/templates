# Svelte 5 - LLM Development Guide

## Core Concepts

### Component Structure
```svelte
<script>
  // Component logic
</script>

<style>
  /* Scoped CSS */
</style>

<!-- HTML template -->
```

### Key Principles
- **Compiler-based**: Svelte compiles to optimized JavaScript
- **Reactive by default**: Changes automatically trigger UI updates
- **Minimal runtime**: Small bundle size, fast performance
- **Component-scoped styles**: CSS only affects current component

## Runes System (Svelte 5)

### $state - Reactive State
```svelte
<script>
  let count = $state(0);
  
  // Deep reactivity for objects/arrays
  let todos = $state([
    { done: false, text: 'Learn Svelte' }
  ]);
</script>

<button onclick={() => count++}>
  Clicks: {count}
</button>
```

**Best Practices:**
- Use for reactive data that triggers UI updates
- Arrays/objects become deep reactive proxies
- **Avoid destructuring** - breaks reactivity
- Use `$state.raw()` for non-reactive large objects

### $derived - Computed Values
```svelte
<script>
  let count = $state(0);
  let doubled = $derived(count * 2);
  
  // Complex derivations
  let total = $derived.by(() => {
    let sum = 0;
    for (const item of items) {
      sum += item.value;
    }
    return sum;
  });
</script>
```

**Best Practices:**
- Use for computed values based on state
- Keep expressions side-effect free
- Use `$derived.by()` for complex calculations
- Can temporarily override values for optimistic UI

### $effect - Side Effects
```svelte
<script>
  let count = $state(0);
  
  $effect(() => {
    console.log(`Count is now ${count}`);
    
    // Cleanup function
    return () => {
      console.log('Cleaning up');
    };
  });
</script>
```

**Best Practices:**
- Use for DOM manipulation, API calls, third-party integrations
- **Never update state inside effects** - causes infinite loops
- Return cleanup functions for subscriptions/intervals
- Only tracks synchronous reads

## Component Patterns

### Props and Events
```svelte
<script>
  let { message, onClick } = $props();
</script>

<button onclick={onClick}>
  {message}
</button>
```

### Class Components
```svelte
<script>
  class Todo {
    done = $state(false);
    
    constructor(text) {
      this.text = $state(text);
    }
    
    // Use arrow functions for methods
    toggle = () => {
      this.done = !this.done;
    }
  }
  
  let todo = new Todo('Learn Svelte');
</script>

<button onclick={todo.toggle}>
  {todo.text} - {todo.done ? 'Done' : 'Pending'}
</button>
```

## Common Patterns

### Sharing State Across Components
```js
// store.js
export const counter = $state({ count: 0 });

export function increment() {
  counter.count += 1;
}

// Alternative: getter pattern
let count = $state(0);

export function getCount() {
  return count;
}
```

### Form Handling
```svelte
<script>
  let form = $state({
    name: '',
    email: ''
  });
  
  function handleSubmit() {
    // Handle form submission
    console.log(form);
  }
</script>

<form onsubmit={handleSubmit}>
  <input bind:value={form.name} placeholder="Name" />
  <input bind:value={form.email} placeholder="Email" />
  <button type="submit">Submit</button>
</form>
```

### List Rendering
```svelte
<script>
  let items = $state([
    { id: 1, name: 'Item 1' },
    { id: 2, name: 'Item 2' }
  ]);
</script>

{#each items as item (item.id)}
  <div>{item.name}</div>
{/each}
```

### TypeScript Support
```svelte
<script lang="ts">
  interface User {
    name: string;
    age: number;
  }
  
  let user: User = $state({ name: 'John', age: 30 });
</script>
```

## Using Classes For Reactive State
Creating a piece of reactive state inside a class works the same:

counter.svelte.ts
```svelte
export class Counter {
  count = $state(0)
  // you can also derive values
  double = $derived(this.count * 2)

  increment = () => this.count++
}
```
You can tuck the class inside a function if you want to hide the new keyword, but I’m going to instantiate the class directly:

+page.svelte
```svelte
<script lang="ts">
  import { Counter } from './counter.svelte'

  const counter = new Counter()
</script>

<button onclick={counter.increment}>
  {counter.count}
</button>
```
Notice how you don’t have to specify a getter and setter for count, since Svelte does that for you:

counter.svelte.ts
```svelte
export class Counter {
  // make count private
  #count = $state(0)

  // create property accessors
  get count() {return this.#count }
  set count(value) { this.#count = value }
}
```
If you’re using TypeScript, you can use type assertion to type a reactive value inside a class:

example.svelte.ts
```svelte
export class Example {
  example = $state() as Type
}
```

## Best Practices for LLMs

1. **Always use runes** in Svelte 5 (`$state`, `$derived`, `$effect`)
2. **Avoid destructuring reactive state** - breaks reactivity
3. **Use arrow functions for class methods** to maintain `this` context
4. **Keep effects clean** - no state updates inside effects
5. **Prefer `$derived.by()`** for complex calculations
6. **Use `bind:this`** for DOM element references
7. **Scoped styles by default** - no need for CSS modules
8. **Component props via `$props()`** destructuring

## Common Gotchas

- **Destructuring breaks reactivity**: `let { count } = state` won't update
- **Method binding**: Use `() => method()` or arrow functions in classes
- **Effect dependencies**: Only synchronous reads are tracked
- **State exports**: Can't directly export reassignable state
- **Async in effects**: Use cleanup functions properly

## File Types

- `.svelte` - Components with HTML, CSS, and JS
- `.svelte.js/.svelte.ts` - Runes-enabled modules for shared logic
- Both support runes but `.svelte.js` files are for pure reactive logic

This guide covers the essential patterns for building Svelte 5 applications efficiently.