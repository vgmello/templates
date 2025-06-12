<script lang="ts">
  import { onMount } from 'svelte';
  import { getOverview, getDashboardStats, getSystemHealth } from '$lib/ResourceClient';
  import { AccountingClient } from '$lib/clients/accounting.client';
  import { BillingClient } from '$lib/clients/billing.client';
  import type { 
    GetOverviewResponse, 
    GetDashboardStatsResponse,
    GetSystemHealthResponse
  } from '$lib/generated/resource_management';
  import type { Ledger } from '$lib/generated/accounting';
  import type { Cashier } from '$lib/generated/billing';

  let overview = $state<GetOverviewResponse | null>(null);
  let dashboardStats = $state<GetDashboardStatsResponse | null>(null);
  let systemHealth = $state<GetSystemHealthResponse | null>(null);
  let recentLedgers = $state<Ledger[]>([]);
  let recentCashiers = $state<Cashier[]>([]);
  let isLoading = $state(true);
  let error = $state<string | null>(null);

  const accountingClient = new AccountingClient(import.meta.env.VITE_ACCOUNTING_API_URL || 'http://localhost:5271');
  const billingClient = new BillingClient(import.meta.env.VITE_BILLING_API_URL || 'http://localhost:5272');

  onMount(async () => {
    await loadDashboardData();
  });

  async function loadDashboardData() {
    try {
      isLoading = true;
      
      // Load recent data from services
      const [ledgersResponse, cashiersResponse] = await Promise.allSettled([
        accountingClient.getLedgers({ limit: 5, offset: 0 }),
        billingClient.getCashiers({ limit: 5, offset: 0, active_only: true })
      ]);

      if (ledgersResponse.status === 'fulfilled') {
        recentLedgers = ledgersResponse.value.ledgers;
      }

      if (cashiersResponse.status === 'fulfilled') {
        recentCashiers = cashiersResponse.value.cashiers;
      }

      // Create mock dashboard stats since the APIs might not be fully implemented
      dashboardStats = {
        total_ledgers: recentLedgers.length || 0,
        total_cashiers: recentCashiers.length || 0,
        total_invoices: 0,
        pending_invoices: 0,
        total_revenue: 0,
        currency: 'USD',
        service_statuses: [
          {
            service_name: 'Accounting API',
            status: ledgersResponse.status === 'fulfilled' ? 'healthy' : 'unhealthy',
            last_check: new Date().toISOString(),
            version: '1.0.0'
          },
          {
            service_name: 'Billing API',
            status: cashiersResponse.status === 'fulfilled' ? 'healthy' : 'unhealthy',
            last_check: new Date().toISOString(),
            version: '1.0.0'
          }
        ]
      };

      error = null;
    } catch (e) {
      console.error('Failed to load dashboard data:', e);
      error = e instanceof Error ? e.message : 'Unknown error occurred';
    } finally {
      isLoading = false;
    }
  }

  function getStatusColor(status: string): string {
    switch (status) {
      case 'healthy': return '#10b981';
      case 'degraded': return '#f59e0b';
      case 'unhealthy': return '#ef4444';
      default: return '#6b7280';
    }
  }
</script>

<svelte:head>
  <title>Operations Dashboard</title>
  <meta name="description" content="Operations management dashboard" />
</svelte:head>

<div class="dashboard">
  <div class="header">
    <h1>Operations Dashboard</h1>
    <button class="btn btn-secondary" onclick={loadDashboardData}>
      Refresh
    </button>
  </div>

  {#if error}
    <div class="alert alert-error">
      <strong>Error:</strong> {error}
      <button class="btn-close" onclick={() => error = null}>×</button>
    </div>
  {/if}

  {#if isLoading}
    <div class="loading">
      <div class="spinner"></div>
      <p>Loading dashboard...</p>
    </div>
  {:else}
    <div class="dashboard-grid">
      <!-- Stats Cards -->
      <div class="stats-section">
        <h2>System Overview</h2>
        {#if dashboardStats}
          <div class="stats-grid">
            <div class="stat-card">
              <div class="stat-value">{dashboardStats.total_ledgers}</div>
              <div class="stat-label">Total Ledgers</div>
            </div>
            <div class="stat-card">
              <div class="stat-value">{dashboardStats.total_cashiers}</div>
              <div class="stat-label">Total Cashiers</div>
            </div>
            <div class="stat-card">
              <div class="stat-value">{dashboardStats.total_invoices}</div>
              <div class="stat-label">Total Invoices</div>
            </div>
            <div class="stat-card">
              <div class="stat-value">{dashboardStats.pending_invoices}</div>
              <div class="stat-label">Pending Invoices</div>
            </div>
          </div>
        {/if}
      </div>

      <!-- Service Status -->
      <div class="service-status-section">
        <h2>Service Health</h2>
        {#if dashboardStats?.service_statuses}
          <div class="service-list">
            {#each dashboardStats.service_statuses as service}
              <div class="service-item">
                <div class="service-info">
                  <h3>{service.service_name}</h3>
                  <p>Version {service.version}</p>
                </div>
                <div class="service-status">
                  <span 
                    class="status-indicator"
                    style="background-color: {getStatusColor(service.status)}"
                  ></span>
                  <span class="status-text">{service.status}</span>
                </div>
              </div>
            {/each}
          </div>
        {/if}
      </div>

      <!-- Recent Ledgers -->
      <div class="recent-section">
        <div class="section-header">
          <h2>Recent Ledgers</h2>
          <a href="/ledgers" class="btn btn-outline">View All</a>
        </div>
        {#if recentLedgers.length > 0}
          <div class="recent-list">
            {#each recentLedgers as ledger}
              <div class="recent-item">
                <div class="item-info">
                  <h4>Ledger {ledger.ledger_id.slice(-8)}</h4>
                  <p>Client: {ledger.client_id.slice(-8)}</p>
                </div>
                <div class="item-meta">
                  <span class="ledger-type">Type {ledger.ledger_type}</span>
                </div>
              </div>
            {/each}
          </div>
        {:else}
          <p class="no-data">No ledgers found</p>
        {/if}
      </div>

      <!-- Recent Cashiers -->
      <div class="recent-section">
        <div class="section-header">
          <h2>Recent Cashiers</h2>
          <a href="/cashiers" class="btn btn-outline">View All</a>
        </div>
        {#if recentCashiers.length > 0}
          <div class="recent-list">
            {#each recentCashiers as cashier}
              <div class="recent-item">
                <div class="item-info">
                  <h4>{cashier.name}</h4>
                  <p>{cashier.email}</p>
                </div>
                <div class="item-meta">
                  <span class="status-badge {cashier.is_active ? 'active' : 'inactive'}">
                    {cashier.is_active ? 'Active' : 'Inactive'}
                  </span>
                </div>
              </div>
            {/each}
          </div>
        {:else}
          <p class="no-data">No cashiers found</p>
        {/if}
      </div>

      <!-- Quick Actions -->
      <div class="actions-section">
        <h2>Quick Actions</h2>
        <div class="actions-grid">
          <a href="/ledgers" class="action-card">
            <h3>Manage Ledgers</h3>
            <p>View and manage accounting ledgers</p>
          </a>
          <a href="/cashiers" class="action-card">
            <h3>Manage Cashiers</h3>
            <p>View and manage billing cashiers</p>
          </a>
        </div>
      </div>
    </div>
  {/if}
</div>

<style>
  .dashboard {
    max-width: 1400px;
    margin: 0 auto;
    padding: 20px;
  }

  .header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 32px;
  }

  .header h1 {
    margin: 0;
    color: #1f2937;
    font-size: 2.5rem;
    font-weight: 700;
  }

  .dashboard-grid {
    display: grid;
    grid-template-columns: 2fr 1fr;
    gap: 24px;
  }

  .stats-section {
    grid-column: 1 / -1;
  }

  .stats-section h2 {
    margin: 0 0 16px 0;
    color: #1f2937;
    font-size: 1.5rem;
    font-weight: 600;
  }

  .stats-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 16px;
  }

  .stat-card {
    background: white;
    border: 1px solid #e5e7eb;
    border-radius: 12px;
    padding: 24px;
    text-align: center;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  }

  .stat-value {
    font-size: 2.5rem;
    font-weight: 700;
    color: #3b82f6;
    margin-bottom: 8px;
  }

  .stat-label {
    color: #6b7280;
    font-size: 0.875rem;
    font-weight: 500;
  }

  .service-status-section,
  .recent-section,
  .actions-section {
    background: white;
    border: 1px solid #e5e7eb;
    border-radius: 12px;
    padding: 24px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  }

  .service-status-section h2,
  .recent-section h2,
  .actions-section h2 {
    margin: 0 0 16px 0;
    color: #1f2937;
    font-size: 1.25rem;
    font-weight: 600;
  }

  .section-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 16px;
  }

  .service-list {
    display: flex;
    flex-direction: column;
    gap: 12px;
  }

  .service-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 12px;
    background: #f9fafb;
    border-radius: 8px;
  }

  .service-info h3 {
    margin: 0;
    color: #1f2937;
    font-size: 0.875rem;
    font-weight: 500;
  }

  .service-info p {
    margin: 4px 0 0 0;
    color: #6b7280;
    font-size: 0.75rem;
  }

  .service-status {
    display: flex;
    align-items: center;
    gap: 8px;
  }

  .status-indicator {
    width: 8px;
    height: 8px;
    border-radius: 50%;
  }

  .status-text {
    font-size: 0.875rem;
    font-weight: 500;
    text-transform: capitalize;
  }

  .recent-list {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }

  .recent-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 12px;
    background: #f9fafb;
    border-radius: 8px;
  }

  .item-info h4 {
    margin: 0;
    color: #1f2937;
    font-size: 0.875rem;
    font-weight: 500;
  }

  .item-info p {
    margin: 4px 0 0 0;
    color: #6b7280;
    font-size: 0.75rem;
  }

  .ledger-type {
    background: #dbeafe;
    color: #1e40af;
    padding: 2px 6px;
    border-radius: 4px;
    font-size: 0.75rem;
    font-weight: 500;
  }

  .status-badge {
    padding: 2px 6px;
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

  .actions-grid {
    display: grid;
    gap: 12px;
  }

  .action-card {
    display: block;
    padding: 16px;
    background: #f9fafb;
    border: 1px solid #e5e7eb;
    border-radius: 8px;
    text-decoration: none;
    transition: all 0.2s;
  }

  .action-card:hover {
    background: #f3f4f6;
    border-color: #d1d5db;
  }

  .action-card h3 {
    margin: 0 0 4px 0;
    color: #1f2937;
    font-size: 0.875rem;
    font-weight: 500;
  }

  .action-card p {
    margin: 0;
    color: #6b7280;
    font-size: 0.75rem;
  }

  .no-data {
    color: #9ca3af;
    font-style: italic;
    text-align: center;
    padding: 20px;
    margin: 0;
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

  .btn-secondary {
    background: #6b7280;
    color: white;
  }

  .btn-secondary:hover {
    background: #4b5563;
  }

  .btn-outline {
    background: transparent;
    color: #3b82f6;
    border: 1px solid #3b82f6;
  }

  .btn-outline:hover {
    background: #3b82f6;
    color: white;
  }

  @media (max-width: 768px) {
    .dashboard-grid {
      grid-template-columns: 1fr;
    }
    
    .stats-grid {
      grid-template-columns: repeat(2, 1fr);
    }
    
    .header {
      flex-direction: column;
      gap: 16px;
      align-items: stretch;
    }
  }
</style>
