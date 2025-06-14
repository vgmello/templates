<script lang="ts">
  import { onMount } from 'svelte';
  import { getResource } from '$lib/ResourceClient'; // Adjusted path assuming $lib alias is to src/lib
  import type { GetResourceResponse } from '$lib/generated/resource_management'; // Adjusted path

  let resource: GetResourceResponse | null = null;
  let error: any = null;
  let isLoading = true;

  onMount(async () => {
    try {
      // For now, let's request a resource with a hardcoded ID.
      // This can be made dynamic later (e.g., from user input).
      resource = await getResource('test-id-123');
    } catch (e) {
      console.error('Failed to load resource:', e);
      error = e;
    } finally {
      isLoading = false;
    }
  });
</script>

<svelte:head>
  <title>Resource Management</title>
  <meta name="description" content="Manage your resources" />
</svelte:head>

<section>
  <h1>Resource Management</h1>

  {#if isLoading}
    <p>Loading resource...</p>
  {:else if error}
    <p style="color: red;">Error loading resource: {error.message || 'Unknown error'}</p>
    <p><small>Check the browser console and the API logs for more details.</small></p>
    <p><small>Ensure the API is running and accessible at the configured VITE_API_URL.</small></p>
  {:else if resource}
    <div>
      <h2>Resource Details</h2>
      <p><strong>ID:</strong> {resource.resourceId}</p>
      <p><strong>Name:</strong> {resource.name}</p>
      <p><strong>Description:</strong> {resource.description}</p>
    </div>
  {:else}
    <p>No resource data found.</p>
  {/if}
</section>

<style>
  section {
    font-family: Arial, sans-serif;
    padding: 20px;
    max-width: 600px;
    margin: auto;
  }
  h1 {
    color: #333;
  }
  p {
    line-height: 1.6;
  }
  strong {
    font-weight: bold;
  }
</style>
