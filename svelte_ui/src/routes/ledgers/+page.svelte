<script lang="ts">
  import { onMount } from 'svelte';
  import { AccountingClient } from '$lib/clients/accounting.client';
  import type { Ledger, CreateLedgerRequest } from '$lib/generated/accounting';

  let ledgers = $state<Ledger[]>([]);
  let isLoading = $state(true);
  let error = $state<string | null>(null);
  let showCreateForm = $state(false);
  
  // Form state
  let newLedger = $state({
    client_id: '',
    ledger_type: 1
  });

  const accountingClient = new AccountingClient(import.meta.env.VITE_ACCOUNTING_API_URL || 'http://localhost:5271');

  onMount(async () => {
    await loadLedgers();
  });

  async function loadLedgers() {
    try {
      isLoading = true;
      const response = await accountingClient.getLedgers({ limit: 50, offset: 0 });
      ledgers = response.ledgers;
      error = null;
    } catch (e) {
      console.error('Failed to load ledgers:', e);
      error = e instanceof Error ? e.message : 'Unknown error occurred';
    } finally {
      isLoading = false;
    }
  }

  async function createLedger() {
    try {
      if (!newLedger.client_id) {
        error = 'Client ID is required';
        return;
      }

      const request: CreateLedgerRequest = {
        client_id: newLedger.client_id,
        ledger_type: newLedger.ledger_type
      };

      const createdLedger = await accountingClient.createLedger(request);
      ledgers = [...ledgers, createdLedger];
      
      // Reset form
      newLedger = { client_id: '', ledger_type: 1 };
      showCreateForm = false;
      error = null;
    } catch (e) {
      console.error('Failed to create ledger:', e);
      error = e instanceof Error ? e.message : 'Failed to create ledger';
    }
  }

  async function deleteLedger(ledgerId: string) {
    try {
      await accountingClient.deleteLedger({ ledger_id: ledgerId });
      ledgers = ledgers.filter(l => l.ledger_id !== ledgerId);
      error = null;
    } catch (e) {
      console.error('Failed to delete ledger:', e);
      error = e instanceof Error ? e.message : 'Failed to delete ledger';
    }
  }

  function getLedgerTypeName(type: number): string {
    const types = {
      1: 'General',
      2: 'Savings',
      3: 'Investment',
      4: 'Credit'
    };
    return types[type as keyof typeof types] || 'Unknown';
  }
</script>

<svelte:head>
  <title>Ledgers Management</title>
  <meta name="description" content="Manage accounting ledgers" />
</svelte:head>

<div class="container">
  <div class="header">
    <h1>Ledgers Management</h1>
    <button 
      class="btn btn-primary"
      onclick={() => showCreateForm = !showCreateForm}
    >
      {showCreateForm ? 'Cancel' : 'Create New Ledger'}
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
      <h2>Create New Ledger</h2>
      <form onsubmit|preventDefault={createLedger}>
        <div class="form-group">
          <label for="client-id">Client ID:</label>
          <input
            id="client-id"
            type="text"
            bind:value={newLedger.client_id}
            placeholder="Enter client ID (UUID)"
            required
          />
        </div>
        
        <div class="form-group">
          <label for="ledger-type">Ledger Type:</label>
          <select id="ledger-type" bind:value={newLedger.ledger_type}>
            <option value={1}>General</option>
            <option value={2}>Savings</option>
            <option value={3}>Investment</option>
            <option value={4}>Credit</option>
          </select>
        </div>
        
        <div class="form-actions">
          <button type="submit" class="btn btn-primary">Create Ledger</button>
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
        <p>Loading ledgers...</p>
      </div>
    {:else if ledgers.length === 0}
      <div class="empty-state">
        <h2>No Ledgers Found</h2>
        <p>Create your first ledger to get started.</p>
      </div>
    {:else}
      <div class="ledgers-grid">
        {#each ledgers as ledger (ledger.ledger_id)}
          <div class="ledger-card">
            <div class="ledger-header">
              <h3>Ledger {ledger.ledger_id.slice(-8)}</h3>
              <span class="ledger-type">{getLedgerTypeName(ledger.ledger_type)}</span>
            </div>
            
            <div class="ledger-details">
              <p><strong>ID:</strong> {ledger.ledger_id}</p>
              <p><strong>Client ID:</strong> {ledger.client_id}</p>
              <p><strong>Type:</strong> {getLedgerTypeName(ledger.ledger_type)}</p>
              {#if ledger.created_at}
                <p><strong>Created:</strong> {new Date(ledger.created_at).toLocaleDateString()}</p>
              {/if}
            </div>

            {#if ledger.balances && ledger.balances.length > 0}
              <div class="balances">
                <h4>Balances:</h4>
                {#each ledger.balances as balance}
                  <div class="balance-item">
                    <span class="currency">{balance.currency}</span>
                    <span class="amount">{balance.amount.toFixed(2)}</span>
                  </div>
                {/each}
              </div>
            {/if}
            
            <div class="ledger-actions">
              <button 
                class="btn btn-danger btn-sm"
                onclick={() => deleteLedger(ledger.ledger_id)}
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

  .form-group input,
  .form-group select {
    width: 100%;
    padding: 8px 12px;
    border: 1px solid #d1d5db;
    border-radius: 4px;
    font-size: 14px;
  }

  .form-group input:focus,
  .form-group select:focus {
    outline: none;
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  }

  .form-actions {
    display: flex;
    gap: 12px;
    margin-top: 20px;
  }

  .ledgers-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
    gap: 20px;
  }

  .ledger-card {
    background: white;
    border: 1px solid #e5e7eb;
    border-radius: 8px;
    padding: 20px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    transition: box-shadow 0.2s;
  }

  .ledger-card:hover {
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  }

  .ledger-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 16px;
  }

  .ledger-header h3 {
    margin: 0;
    color: #1f2937;
    font-size: 1.125rem;
  }

  .ledger-type {
    background: #dbeafe;
    color: #1e40af;
    padding: 4px 8px;
    border-radius: 4px;
    font-size: 0.75rem;
    font-weight: 500;
  }

  .ledger-details p {
    margin: 8px 0;
    color: #6b7280;
    font-size: 0.875rem;
  }

  .ledger-details strong {
    color: #374151;
  }

  .balances {
    margin: 16px 0;
    padding: 12px;
    background: #f9fafb;
    border-radius: 6px;
  }

  .balances h4 {
    margin: 0 0 8px 0;
    font-size: 0.875rem;
    color: #374151;
  }

  .balance-item {
    display: flex;
    justify-content: space-between;
    margin: 4px 0;
  }

  .currency {
    font-weight: 500;
    color: #6b7280;
  }

  .amount {
    font-weight: 600;
    color: #059669;
  }

  .ledger-actions {
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

  .btn-primary {
    background: #3b82f6;
    color: white;
  }

  .btn-primary:hover {
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