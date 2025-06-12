<script lang="ts">
  import Navigation from '$lib/components/Navigation.svelte';
  import '../app.css';
</script>

<div class="app">
  <Navigation />
  <main class="main-content">
    <slot />
  </main>
</div>

<style>
  :global(html) {
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
  }

  :global(body) {
    margin: 0;
    padding: 0;
    background: #f9fafb;
    color: #1f2937;
    line-height: 1.6;
  }

  :global(*) {
    box-sizing: border-box;
  }

  .app {
    min-height: 100vh;
    display: flex;
    flex-direction: column;
  }

  .main-content {
    flex: 1;
  }
</style>