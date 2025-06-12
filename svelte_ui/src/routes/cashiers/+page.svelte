<script lang="ts">
  import { onMount } from 'svelte';
  import { BillingClient } from '$lib/clients/billing.client';
  import type { Cashier, CreateCashierRequest } from '$lib/generated/billing';

  let cashiers = $state<Cashier[]>([]);
  let isLoading = $state(true);
  let error = $state<string | null>(null);
  let showCreateForm = $state(false);
  
  // Form state
  let newCashier = $state({
    name: '',
    email: ''
  });

  const billingClient = new BillingClient(import.meta.env.VITE_BILLING_API_URL || 'http://localhost:5272');

  onMount(async () => {
    await loadCashiers();
  });

  async function loadCashiers() {
    try {
      isLoading = true;
      const response = await billingClient.getCashiers({ limit: 50, offset: 0, active_only: false });
      cashiers = response.cashiers;
      error = null;
    } catch (e) {
      console.error('Failed to load cashiers:', e);
      error = e instanceof Error ? e.message : 'Unknown error occurred';
    } finally {
      isLoading = false;
    }
  }

  async function createCashier() {
    try {
      if (!newCashier.name || !newCashier.email) {
        error = 'Name and email are required';
        return;
      }

      const request: CreateCashierRequest = {
        name: newCashier.name,
        email: newCashier.email
      };

      const createdCashier = await billingClient.createCashier(request);
      cashiers = [...cashiers, createdCashier];
      
      // Reset form
      newCashier = { name: '', email: '' };
      showCreateForm = false;
      error = null;
    } catch (e) {
      console.error('Failed to create cashier:', e);
      error = e instanceof Error ? e.message : 'Failed to create cashier';
    }
  }

  async function deleteCashier(cashierId: string) {
    try {
      await billingClient.deleteCashier({ cashier_id: cashierId });
      cashiers = cashiers.filter(c => c.cashier_id !== cashierId);
      error = null;
    } catch (e) {
      console.error('Failed to delete cashier:', e);
      error = e instanceof Error ? e.message : 'Failed to delete cashier';
    }
  }

  function isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }
</script>

<svelte:head>
  <title>Cashiers Management</title>
  <meta name="description" content="Manage billing cashiers" />
</svelte:head>

<div class="container">
  <div class="header">
    <h1>Cashiers Management</h1>
    <button 
      class="btn btn-primary"
      onclick={() => showCreateForm = !showCreateForm}
    >
      {showCreateForm ? 'Cancel' : 'Create New Cashier'}
    </button>
  </div>

  {#if error}
    <div class="alert alert-error">
      <strong>Error:</strong> {error}
      <button class="btn-close" onclick={() => error = null}>×</button>
    </div>
  {/if}

  {#if showCreateForm}
    <div class="form-container">
      <h2>Create New Cashier</h2>
      <form onsubmit|preventDefault={createCashier}>
        <div class="form-group">
          <label for="cashier-name">Name:</label>
          <input
            id="cashier-name"
            type="text"
            bind:value={newCashier.name}
            placeholder="Enter cashier name"
            required
          />
        </div>
        
        <div class="form-group">
          <label for="cashier-email">Email:</label>
          <input
            id="cashier-email"
            type="email"
            bind:value={newCashier.email}
            placeholder="Enter email address"
            required
          />
        </div>
        
        <div class="form-actions">
          <button 
            type="submit" 
            class="btn btn-primary"
            disabled={!newCashier.name || !newCashier.email || !isValidEmail(newCashier.email)}
          >
            Create Cashier
          </button>
          <button 
            type="button" 
            class="btn btn-secondary"
            onclick={() => showCreateForm = false}
          >
            Cancel
          </button>
        </div>
      </form>
    </div>
  {/if}

  <div class="content">
    {#if isLoading}
      <div class="loading">
        <div class="spinner"></div>
        <p>Loading cashiers...</p>
      </div>
    {:else if cashiers.length === 0}
      <div class="empty-state">
        <h2>No Cashiers Found</h2>
        <p>Create your first cashier to get started.</p>
      </div>
    {:else}
      <div class="cashiers-grid">
        {#each cashiers as cashier (cashier.cashier_id)}
          <div class="cashier-card">
            <div class="cashier-header">
              <h3>{cashier.name}</h3>
              <span class="status-badge {cashier.is_active ? 'active' : 'inactive'}">
                {cashier.is_active ? 'Active' : 'Inactive'}
              </span>
            </div>
            
            <div class="cashier-details">
              <p><strong>ID:</strong> {cashier.cashier_id}</p>
              <p><strong>Email:</strong> {cashier.email}</p>
              {#if cashier.created_at}
                <p><strong>Created:</strong> {new Date(cashier.created_at).toLocaleDateString()}</p>
              {/if}
              {#if cashier.updated_at && cashier.updated_at !== cashier.created_at}
                <p><strong>Updated:</strong> {new Date(cashier.updated_at).toLocaleDateString()}</p>
              {/if}
            </div>

            {#if cashier.currencies && cashier.currencies.length > 0}
              <div class="currencies">
                <h4>Currency Balances:</h4>
                {#each cashier.currencies as currency}
                  <div class="currency-item">
                    <span class="currency-code">{currency.currency}</span>
                    <span class="balance">{currency.balance.toFixed(2)}</span>
                  </div>
                {/each}
              </div>
            {:else}
              <div class="currencies">
                <p class="no-currencies">No currency balances</p>
              </div>
            {/if}
            
            <div class="cashier-actions">
              <button 
                class="btn btn-danger btn-sm"
                onclick={() => deleteCashier(cashier.cashier_id)}
              >
                Delete
              </button>
            </div>
          </div>
        {/each}
      </div>
    {/if}
  </div>
</div>

<style>
  .container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 20px;
  }

  .header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 24px;
  }

  .header h1 {
    margin: 0;
    color: #1f2937;
    font-size: 2rem;
    font-weight: 600;
  }

  .form-container {
    background: #f9fafb;
    border: 1px solid #e5e7eb;
    border-radius: 8px;
    padding: 24px;
    margin-bottom: 24px;
  }

  .form-container h2 {
    margin: 0 0 20px 0;
    color: #1f2937;
    font-size: 1.25rem;
  }

  .form-group {
    margin-bottom: 16px;
  }

  .form-group label {
    display: block;
    margin-bottom: 4px;
    font-weight: 500;
    color: #374151;
  }

  .form-group input {
    width: 100%;
    padding: 8px 12px;
    border: 1px solid #d1d5db;
    border-radius: 4px;
    font-size: 14px;
  }

  .form-group input:focus {
    outline: none;
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  }

  .form-actions {
    display: flex;
    gap: 12px;
    margin-top: 20px;
  }

  .cashiers-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
    gap: 20px;
  }

  .cashier-card {
    background: white;
    border: 1px solid #e5e7eb;
    border-radius: 8px;
    padding: 20px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    transition: box-shadow 0.2s;
  }

  .cashier-card:hover {
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  }

  .cashier-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 16px;
  }

  .cashier-header h3 {
    margin: 0;
    color: #1f2937;
    font-size: 1.125rem;
  }

  .status-badge {
    padding: 4px 8px;
    border-radius: 4px;
    font-size: 0.75rem;
    font-weight: 500;
  }

  .status-badge.active {
    background: #d1fae5;
    color: #065f46;
  }

  .status-badge.inactive {
    background: #fee2e2;
    color: #991b1b;
  }

  .cashier-details p {
    margin: 8px 0;
    color: #6b7280;
    font-size: 0.875rem;
  }

  .cashier-details strong {
    color: #374151;
  }

  .currencies {
    margin: 16px 0;
    padding: 12px;
    background: #f9fafb;
    border-radius: 6px;
  }

  .currencies h4 {
    margin: 0 0 8px 0;
    font-size: 0.875rem;
    color: #374151;
  }

  .currency-item {
    display: flex;
    justify-content: space-between;
    margin: 4px 0;
  }

  .currency-code {
    font-weight: 500;
    color: #6b7280;
  }

  .balance {
    font-weight: 600;
    color: #059669;
  }

  .no-currencies {
    margin: 0;
    color: #9ca3af;
    font-style: italic;
    font-size: 0.875rem;
  }

  .cashier-actions {
    margin-top: 16px;
    padding-top: 16px;
    border-top: 1px solid #e5e7eb;
  }

  .loading {
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 60px 20px;
    color: #6b7280;
  }

  .spinner {
    width: 32px;
    height: 32px;
    border: 3px solid #e5e7eb;
    border-top: 3px solid #3b82f6;
    border-radius: 50%;
    animation: spin 1s linear infinite;
    margin-bottom: 16px;
  }

  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }

  .empty-state {
    text-align: center;
    padding: 60px 20px;
    color: #6b7280;
  }

  .empty-state h2 {
    margin: 0 0 8px 0;
    color: #374151;
  }

  .alert {
    padding: 12px 16px;
    border-radius: 6px;
    margin-bottom: 20px;
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .alert-error {
    background: #fee2e2;
    border: 1px solid #fecaca;
    color: #dc2626;
  }

  .btn-close {
    background: none;
    border: none;
    font-size: 1.25rem;
    cursor: pointer;
    color: inherit;
    padding: 0;
    margin-left: 12px;
  }

  .btn {
    padding: 8px 16px;
    border: none;
    border-radius: 6px;
    font-size: 14px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s;
    text-decoration: none;
    display: inline-block;
  }

  .btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  .btn-primary {
    background: #3b82f6;
    color: white;
  }

  .btn-primary:hover:not(:disabled) {
    background: #2563eb;
  }

  .btn-secondary {
    background: #6b7280;
    color: white;
  }

  .btn-secondary:hover {
    background: #4b5563;
  }

  .btn-danger {
    background: #dc2626;
    color: white;
  }

  .btn-danger:hover {
    background: #b91c1c;
  }

  .btn-sm {
    padding: 6px 12px;
    font-size: 12px;
  }
</style>