<script lang="ts">
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import { Button } from '$lib/components/ui/button';
	import { Card, CardHeader, CardTitle, CardContent } from '$lib/components/ui/card';
	import { Input } from '$lib/components/ui/input';
	import { 
		Plus, Search, Eye, Edit, Trash2, FileText, DollarSign, 
		CheckCircle, AlertCircle, Calendar, Download 
	} from '@lucide/svelte';
	import { invoiceApi, type Invoice, type InvoiceSummary } from '$lib';
	import InvoiceStatusBadge from '$lib/components/InvoiceStatusBadge.svelte';
	import { formatCurrency } from '$lib/utils/currency.js';
	import { formatDate, isOverdue } from '$lib/utils/date.js';

	let invoices = $state<Invoice[]>([]);
	let summary = $state<InvoiceSummary>({ totalInvoices: 0, totalAmount: 0, paidCount: 0, overdueCount: 0 });
	let loading = $state(false);
	let error = $state<string | null>(null);
	let searchTerm = $state('');
	let statusFilter = $state<string>('');

	// Reactive filtered invoices
	let filteredInvoices = $derived(
		invoices.filter(invoice => {
			const matchesSearch = invoice.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
								invoice.invoiceId.toLowerCase().includes(searchTerm.toLowerCase());
			const matchesStatus = !statusFilter || invoice.status === statusFilter;
			return matchesSearch && matchesStatus;
		})
	);

	async function loadInvoices() {
		loading = true;
		error = null;
		
		try {
			const [invoicesData, summaryData] = await Promise.all([
				invoiceApi.getInvoices(),
				invoiceApi.getInvoiceSummary()
			]);
			
			invoices = invoicesData;
			summary = summaryData;
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to load invoices';
			console.error('Error loading invoices:', err);
		} finally {
			loading = false;
		}
	}

	function viewInvoice(id: string) {
		goto(`/invoices/${id}`);
	}

	function createInvoice() {
		goto('/invoices/create');
	}

	// Determine if an invoice should be marked as overdue
	function getInvoiceStatus(invoice: Invoice): string {
		if (invoice.status === 'Paid' || invoice.status === 'Cancelled') {
			return invoice.status;
		}
		return isOverdue(invoice.dueDate) ? 'Overdue' : invoice.status;
	}

	onMount(() => {
		loadInvoices();
	});
</script>

<svelte:head>
	<title>Invoice Management - Billing System</title>
</svelte:head>

<div class="container mx-auto p-6 space-y-8">
	<!-- Header Section -->
	<div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
		<div class="space-y-1">
			<h1 class="text-3xl font-bold tracking-tight text-foreground">Invoices</h1>
			<p class="text-muted-foreground">
				Manage and track your invoices. {filteredInvoices.length} total, {summary.paidCount} paid.
			</p>
		</div>
		<Button onclick={createInvoice} class="flex items-center gap-2 h-10 px-6">
			<Plus size={18} />
			Create Invoice
		</Button>
	</div>

	{#if loading}
		<div class="flex flex-col items-center justify-center py-24 space-y-4">
			<div class="animate-spin rounded-full h-12 w-12 border-2 border-primary border-t-transparent"></div>
			<div class="text-center space-y-1">
				<p class="font-medium">Loading invoices</p>
				<p class="text-sm text-muted-foreground">Please wait while we fetch your data...</p>
			</div>
		</div>
	{:else if error}
		<Card class="border-destructive/50 bg-destructive/5">
			<CardContent class="flex flex-col items-center justify-center py-12 space-y-4">
				<div class="rounded-full bg-destructive/10 p-3">
					<AlertCircle size={24} class="text-destructive" />
				</div>
				<div class="text-center space-y-2">
					<h3 class="font-semibold text-destructive">Something went wrong</h3>
					<p class="text-sm text-muted-foreground max-w-md">{error}</p>
				</div>
				<Button onclick={loadInvoices} variant="outline" class="gap-2">
					<AlertCircle size={16} />
					Try Again
				</Button>
			</CardContent>
		</Card>
	{:else}
		<!-- Summary Cards -->
		<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
			<Card>
				<CardContent class="p-6">
					<div class="flex items-center justify-between">
						<div class="space-y-1">
							<p class="text-sm font-medium text-muted-foreground">Total Invoices</p>
							<p class="text-2xl font-bold">{summary.totalInvoices}</p>
						</div>
						<div class="rounded-full bg-blue-100 p-3">
							<FileText size={20} class="text-blue-600" />
						</div>
					</div>
				</CardContent>
			</Card>

			<Card>
				<CardContent class="p-6">
					<div class="flex items-center justify-between">
						<div class="space-y-1">
							<p class="text-sm font-medium text-muted-foreground">Total Amount</p>
							<p class="text-2xl font-bold">{formatCurrency(summary.totalAmount)}</p>
						</div>
						<div class="rounded-full bg-green-100 p-3">
							<DollarSign size={20} class="text-green-600" />
						</div>
					</div>
				</CardContent>
			</Card>

			<Card>
				<CardContent class="p-6">
					<div class="flex items-center justify-between">
						<div class="space-y-1">
							<p class="text-sm font-medium text-muted-foreground">Paid</p>
							<p class="text-2xl font-bold text-green-600">{summary.paidCount}</p>
						</div>
						<div class="rounded-full bg-green-100 p-3">
							<CheckCircle size={20} class="text-green-600" />
						</div>
					</div>
				</CardContent>
			</Card>

			<Card>
				<CardContent class="p-6">
					<div class="flex items-center justify-between">
						<div class="space-y-1">
							<p class="text-sm font-medium text-muted-foreground">Overdue</p>
							<p class="text-2xl font-bold text-orange-600">{summary.overdueCount}</p>
						</div>
						<div class="rounded-full bg-orange-100 p-3">
							<AlertCircle size={20} class="text-orange-600" />
						</div>
					</div>
				</CardContent>
			</Card>
		</div>

		<!-- Search and Filter Bar -->
		<div class="flex flex-col sm:flex-row gap-4 items-stretch sm:items-center">
			<div class="relative flex-1 max-w-md">
				<Search size={16} class="absolute left-3 top-1/2 transform -translate-y-1/2 text-muted-foreground" />
				<Input
					bind:value={searchTerm}
					placeholder="Search by invoice name or ID..."
					class="pl-10 h-10"
				/>
			</div>
			<div class="flex items-center gap-2">
				<select 
					bind:value={statusFilter}
					class="px-3 py-2 border border-input bg-background text-sm rounded-md focus:outline-none focus:ring-2 focus:ring-ring"
				>
					<option value="">All Statuses</option>
					<option value="Draft">Draft</option>
					<option value="Pending">Pending</option>
					<option value="Paid">Paid</option>
					<option value="Cancelled">Cancelled</option>
				</select>
			</div>
		</div>

		{#if filteredInvoices.length === 0}
			<Card class="border-dashed border-2">
				<CardContent class="flex flex-col items-center justify-center py-24 space-y-6">
					<div class="rounded-full bg-muted p-4">
						<FileText size={32} class="text-muted-foreground" />
					</div>
					<div class="text-center space-y-2 max-w-md">
						<h3 class="text-lg font-semibold">
							{searchTerm || statusFilter ? 'No matching invoices' : 'No invoices found'}
						</h3>
						<p class="text-muted-foreground">
							{searchTerm || statusFilter 
								? `No invoices found matching your search criteria. Try adjusting your filters.`
								: 'Get started by creating your first invoice to track payments and manage billing.'
							}
						</p>
					</div>
					{#if !searchTerm && !statusFilter}
						<Button onclick={createInvoice} class="gap-2">
							<Plus size={16} />
							Create Your First Invoice
						</Button>
					{/if}
				</CardContent>
			</Card>
		{:else}
			<!-- Invoices Table -->
			<Card>
				<CardContent class="p-0">
					<div class="overflow-x-auto">
						<table class="w-full">
							<thead class="border-b">
								<tr class="text-left">
									<th class="py-4 px-6 font-medium text-muted-foreground">Invoice</th>
									<th class="py-4 px-6 font-medium text-muted-foreground">Amount</th>
									<th class="py-4 px-6 font-medium text-muted-foreground">Status</th>
									<th class="py-4 px-6 font-medium text-muted-foreground">Due Date</th>
									<th class="py-4 px-6 font-medium text-muted-foreground">Created</th>
									<th class="py-4 px-6 font-medium text-muted-foreground">Actions</th>
								</tr>
							</thead>
							<tbody>
								{#each filteredInvoices as invoice}
									<tr class="border-b hover:bg-muted/50 transition-colors">
										<td class="py-4 px-6">
											<div class="space-y-1">
												<p class="font-medium">{invoice.name}</p>
												<p class="text-sm text-muted-foreground font-mono">
													{invoice.invoiceId.slice(0, 8)}...
												</p>
											</div>
										</td>
										<td class="py-4 px-6">
											<div class="space-y-1">
												<p class="font-medium">{formatCurrency(invoice.amount, invoice.currency)}</p>
												<p class="text-sm text-muted-foreground">{invoice.currency || 'USD'}</p>
											</div>
										</td>
										<td class="py-4 px-6">
											<InvoiceStatusBadge status={getInvoiceStatus(invoice)} />
										</td>
										<td class="py-4 px-6">
											{#if invoice.dueDate}
												<div class="space-y-1">
													<p class="text-sm">{formatDate(invoice.dueDate, 'short')}</p>
													{#if isOverdue(invoice.dueDate) && invoice.status !== 'Paid' && invoice.status !== 'Cancelled'}
														<p class="text-xs text-orange-600">Overdue</p>
													{/if}
												</div>
											{:else}
												<span class="text-muted-foreground text-sm">No due date</span>
											{/if}
										</td>
										<td class="py-4 px-6">
											<p class="text-sm">{formatDate(invoice.createdDateUtc, 'short')}</p>
										</td>
										<td class="py-4 px-6">
											<div class="flex items-center gap-2">
												<Button
													size="sm"
													variant="outline"
													onclick={() => viewInvoice(invoice.invoiceId)}
													class="gap-1"
												>
													<Eye size={14} />
													View
												</Button>
											</div>
										</td>
									</tr>
								{/each}
							</tbody>
						</table>
					</div>
				</CardContent>
			</Card>

			<!-- Summary Footer -->
			<div class="flex items-center justify-between pt-6 border-t">
				<p class="text-sm text-muted-foreground">
					Showing {filteredInvoices.length} of {invoices.length} invoices
				</p>
				<div class="flex items-center gap-4 text-sm text-muted-foreground">
					<span class="flex items-center gap-1">
						<div class="w-2 h-2 rounded-full bg-green-500"></div>
						{summary.paidCount} paid
					</span>
					<span class="flex items-center gap-1">
						<div class="w-2 h-2 rounded-full bg-orange-500"></div>
						{summary.overdueCount} overdue
					</span>
				</div>
			</div>
		{/if}
	{/if}
</div>