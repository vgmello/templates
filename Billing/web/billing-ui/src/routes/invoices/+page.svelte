<script lang="ts">
	import { onMount } from 'svelte';
	import { invoiceStore } from '$lib/stores/invoice.svelte.js';
	import Button from '$lib/components/ui/button.svelte';
	import Card from '$lib/components/ui/card.svelte';
	import Input from '$lib/components/ui/input.svelte';
	import Badge from '$lib/components/ui/badge.svelte';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();

	// Initialize store with SSR data
	onMount(() => {
		invoiceStore.initializeInvoices(data.invoices);
		if (data.error) {
			invoiceStore.setError(data.error);
		}
	});

	// Format currency
	function formatCurrency(amount: number, currency: string): string {
		return new Intl.NumberFormat('en-US', {
			style: 'currency',
			currency: currency || 'USD'
		}).format(amount);
	}

	// Format date
	function formatDate(dateString: string | undefined): string {
		if (!dateString) return '';
		return new Date(dateString).toLocaleDateString('en-US', {
			year: 'numeric',
			month: 'short',
			day: 'numeric'
		});
	}

	// Get status badge variant
	function getStatusVariant(status: string): 'default' | 'secondary' | 'destructive' | 'outline' {
		switch (status) {
			case 'Paid': return 'default';
			case 'Draft': return 'secondary';
			case 'Cancelled': return 'destructive';
			default: return 'outline';
		}
	}

	// Check if invoice is overdue
	function isOverdue(invoice: any): boolean {
		if (!invoice.dueDate || invoice.status === 'Paid' || invoice.status === 'Cancelled') return false;
		return new Date(invoice.dueDate) < new Date();
	}
</script>

<svelte:head>
	<title>Invoices - Billing System</title>
	<meta name="description" content="Manage invoices in the billing system" />
</svelte:head>

<div class="container mx-auto px-4 py-8">
	<!-- Header -->
	<div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between mb-8">
		<div>
			<h1 class="text-3xl font-bold tracking-tight">Invoices</h1>
			<p class="text-muted-foreground">
				Manage and track your invoices
			</p>
		</div>
		<a href="/invoices/create" class="inline-flex items-center justify-center whitespace-nowrap rounded-md text-sm font-medium ring-offset-background transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50 bg-primary text-primary-foreground hover:bg-primary/90 h-10 px-4 py-2 w-full sm:w-auto">
			Create Invoice
		</a>
	</div>

	<!-- Statistics Cards -->
	<div class="grid gap-4 md:grid-cols-2 lg:grid-cols-4 mb-8">
		<Card class="p-4">
			<div class="flex flex-row items-center justify-between space-y-0 pb-2">
				<h3 class="text-sm font-medium">Total Invoices</h3>
			</div>
			<div class="text-2xl font-bold">{invoiceStore.totalInvoices}</div>
		</Card>
		
		<Card class="p-4">
			<div class="flex flex-row items-center justify-between space-y-0 pb-2">
				<h3 class="text-sm font-medium">Total Amount</h3>
			</div>
			<div class="text-2xl font-bold">{formatCurrency(invoiceStore.totalAmount, 'USD')}</div>
		</Card>
		
		<Card class="p-4">
			<div class="flex flex-row items-center justify-between space-y-0 pb-2">
				<h3 class="text-sm font-medium">Paid</h3>
			</div>
			<div class="text-2xl font-bold text-green-600">{invoiceStore.paidInvoices}</div>
		</Card>
		
		<Card class="p-4">
			<div class="flex flex-row items-center justify-between space-y-0 pb-2">
				<h3 class="text-sm font-medium">Overdue</h3>
			</div>
			<div class="text-2xl font-bold text-red-600">{invoiceStore.getOverdueInvoices().length}</div>
		</Card>
	</div>

	<!-- Filters -->
	<div class="flex flex-col gap-4 sm:flex-row sm:items-center mb-6">
		<div class="flex-1">
			<Input
				type="search"
				placeholder="Search by invoice name or ID..."
				bind:value={invoiceStore.searchTerm}
				class="max-w-sm"
			/>
		</div>
		
		<div class="flex gap-2">
			<select
				bind:value={invoiceStore.statusFilter}
				class="px-3 py-2 border border-input bg-background rounded-md text-sm"
			>
				<option value="">All Statuses</option>
				{#each invoiceStore.availableStatuses as status}
					<option value={status}>{status}</option>
				{/each}
			</select>
		</div>
	</div>

	<!-- Error Message -->
	{#if invoiceStore.errorMessage}
		<div class="bg-destructive/15 text-destructive px-4 py-3 rounded-md mb-6">
			{invoiceStore.errorMessage}
			<Button variant="ghost" size="sm" onclick={() => invoiceStore.clearError()} class="ml-2">
				Dismiss
			</Button>
		</div>
	{/if}

	<!-- Invoices List -->
	<div class="space-y-4">
		{#each invoiceStore.filteredInvoices as invoice (invoice.invoiceId)}
			<Card class="hover:shadow-md transition-shadow p-6">
				<div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
					<div class="flex-1">
						<div class="flex items-center gap-3 mb-2">
							<h3 class="font-semibold text-lg">
								<a 
									href="/invoices/{invoice.invoiceId}" 
									class="hover:underline focus:underline focus:outline-none"
								>
									{invoice.name}
								</a>
							</h3>
							<Badge class={getStatusVariant(invoice.status) === 'destructive' ? 'bg-red-100 text-red-800' : getStatusVariant(invoice.status) === 'default' ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'}>
								{invoice.status}
							</Badge>
							{#if isOverdue(invoice)}
								<Badge class="bg-red-100 text-red-800">Overdue</Badge>
							{/if}
						</div>
						
						<div class="text-sm text-muted-foreground space-y-1">
							<p>Invoice ID: {invoice.invoiceId}</p>
							<p>Amount: {formatCurrency(invoice.amount, invoice.currency)}</p>
							{#if invoice.dueDate}
								<p>Due Date: {formatDate(invoice.dueDate)}</p>
							{/if}
							<p>Created: {formatDate(invoice.createdDateUtc)}</p>
						</div>
					</div>
					
					<div class="flex gap-2">
						{#if invoice.status === 'Draft'}
							<Button variant="outline" size="sm" onclick={() => window.location.href = `/invoices/${invoice.invoiceId}/edit`}>
								Edit
							</Button>
						{/if}
						<Button variant="outline" size="sm" onclick={() => window.location.href = `/invoices/${invoice.invoiceId}`}>
							View Details
						</Button>
					</div>
				</div>
			</Card>
		{:else}
			<Card class="p-8 text-center">
				<p class="text-muted-foreground mb-4">
					{invoiceStore.searchTerm || invoiceStore.statusFilter ? 'No invoices match your search criteria.' : 'No invoices found.'}
				</p>
				<a href="/invoices/create" class="inline-flex items-center justify-center whitespace-nowrap rounded-md text-sm font-medium ring-offset-background transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50 bg-primary text-primary-foreground hover:bg-primary/90 h-10 px-4 py-2">Create Your First Invoice</a>
			</Card>
		{/each}
	</div>
</div>