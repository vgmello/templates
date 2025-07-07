<script lang="ts">
	import { goto } from '$app/navigation';
	import { Button } from '$lib/components/ui/button';
	import { Card, CardHeader, CardTitle, CardContent } from '$lib/components/ui/card';
	import { Input } from '$lib/components/ui/input';
	import { CurrencyDisplay } from '$lib/components/ui/currency-display';
	import { Select } from '$lib/components/ui/select';
	import {
		Plus,
		Search,
		Eye,
		Edit,
		Trash2,
		FileText,
		DollarSign,
		CheckCircle,
		AlertCircle,
		Calendar,
		Download,
		Copy
	} from '@lucide/svelte';
	import type { Invoice, InvoiceSummary } from '$lib';
	import InvoiceStatusBadge from '$lib/components/InvoiceStatusBadge.svelte';
	import { formatCurrency } from '$lib/utils/currency.js';
	import { formatDate, isOverdue } from '$lib/utils/date.js';

	type Props = {
		data: {
			invoices: Invoice[];
			summary: InvoiceSummary;
		};
	};
	
	let { data }: Props = $props();
	let { invoices, summary } = data;
	let searchTerm = $state('');
	let statusFilter = $state<string>('');

	// Status filter options
	const statusOptions = [
		{ value: '', label: 'All Statuses' },
		{ value: 'Draft', label: 'Draft' },
		{ value: 'Pending', label: 'Pending' },
		{ value: 'Paid', label: 'Paid' },
		{ value: 'Cancelled', label: 'Cancelled' }
	];

	// Reactive filtered invoices
	let filteredInvoices = $derived(
		invoices.filter((invoice) => {
			const matchesSearch =
				invoice.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
				invoice.invoiceId.toLowerCase().includes(searchTerm.toLowerCase());
			const matchesStatus = !statusFilter || invoice.status === statusFilter;
			return matchesSearch && matchesStatus;
		})
	);


	function viewInvoice(id: string) {
		goto(`/invoices/${id}`);
	}

	function createInvoice() {
		goto('/invoices/create');
	}

	// Copy invoice ID to clipboard
	async function copyInvoiceId(invoiceId: string) {
		try {
			await navigator.clipboard.writeText(invoiceId);
			// You could add a toast notification here if you have one
			console.log('Invoice ID copied to clipboard:', invoiceId);
		} catch (err) {
			console.error('Failed to copy invoice ID:', err);
			// Fallback for older browsers
			const textArea = document.createElement('textarea');
			textArea.value = invoiceId;
			document.body.appendChild(textArea);
			textArea.select();
			document.execCommand('copy');
			document.body.removeChild(textArea);
		}
	}

	// Determine if an invoice should be marked as overdue
	function getInvoiceStatus(invoice: Invoice): string {
		if (invoice.status === 'Paid' || invoice.status === 'Cancelled') {
			return invoice.status;
		}
		return isOverdue(invoice.dueDate) ? 'Overdue' : invoice.status;
	}

</script>

<svelte:head>
	<title>Invoice Management - Billing System</title>
</svelte:head>

<div class="container mx-auto space-y-8 p-6">
	<!-- Header Section -->
	<div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
		<div class="space-y-1">
			<h1 class="text-foreground text-3xl font-bold tracking-tight">Invoices</h1>
			<p class="text-muted-foreground">
				Manage and track your invoices. {filteredInvoices.length} total, {summary.paidCount}
				paid.
			</p>
		</div>
		<Button onclick={createInvoice} class="flex h-10 items-center gap-2 px-6">
			<Plus size={18} />
			Create Invoice
		</Button>
	</div>

	<!-- Summary Cards -->
		<!-- Summary Cards -->
		<div class="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-4">
			<Card>
				<CardContent class="p-6">
					<div class="flex items-center justify-between">
						<div class="space-y-1">
							<p class="text-muted-foreground text-sm font-medium">Total Invoices</p>
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
							<p class="text-muted-foreground text-sm font-medium">Total Amount</p>
							<CurrencyDisplay amount={summary.totalAmount} size="xl" />
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
							<p class="text-muted-foreground text-sm font-medium">Paid</p>
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
							<p class="text-muted-foreground text-sm font-medium">Overdue</p>
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
		<div class="flex flex-col items-stretch gap-4 sm:flex-row sm:items-center">
			<div class="relative max-w-md flex-1">
				<Search
					size={16}
					class="text-muted-foreground absolute left-3 top-1/2 -translate-y-1/2 transform"
				/>
				<Input
					bind:value={searchTerm}
					placeholder="Search by invoice name or ID..."
					class="h-10 pl-10"
				/>
			</div>
			<div class="flex items-center gap-2">
				<Select
					bind:value={statusFilter}
					options={statusOptions}
					placeholder="Filter by status"
					class="min-w-[140px]"
				/>
			</div>
		</div>

		{#if filteredInvoices.length === 0}
			<Card class="border-2 border-dashed">
				<CardContent class="flex flex-col items-center justify-center space-y-6 py-24">
					<div class="bg-muted rounded-full p-4">
						<FileText size={32} class="text-muted-foreground" />
					</div>
					<div class="max-w-md space-y-2 text-center">
						<h3 class="text-lg font-semibold">
							{searchTerm || statusFilter
								? 'No matching invoices'
								: 'No invoices found'}
						</h3>
						<p class="text-muted-foreground">
							{searchTerm || statusFilter
								? `No invoices found matching your search criteria. Try adjusting your filters.`
								: 'Get started by creating your first invoice to track payments and manage billing.'}
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
									<th class="text-muted-foreground px-6 py-4 font-medium"
										>Invoice</th
									>
									<th class="text-muted-foreground px-6 py-4 font-medium"
										>Amount</th
									>
									<th class="text-muted-foreground px-6 py-4 font-medium"
										>Status</th
									>
									<th class="text-muted-foreground px-6 py-4 font-medium"
										>Due Date</th
									>
									<th class="text-muted-foreground px-6 py-4 font-medium"
										>Created</th
									>
									<th class="text-muted-foreground px-6 py-4 font-medium"
										>Actions</th
									>
								</tr>
							</thead>
							<tbody>
								{#each filteredInvoices as invoice}
									<tr class="hover:bg-muted/50 border-b transition-colors">
										<td class="px-6 py-4">
											<div class="space-y-1">
												<p class="font-medium">{invoice.name}</p>
												<div class="flex items-center gap-1">
													<button
														onclick={() =>
															copyInvoiceId(invoice.invoiceId)}
														title="Click to copy full ID: {invoice.invoiceId}"
														class="text-muted-foreground hover:text-foreground group flex items-center gap-1 font-mono text-sm transition-colors"
													>
														<span>{invoice.invoiceId.slice(0, 8)}</span>
														<Copy size={12} />
													</button>
												</div>
											</div>
										</td>
										<td class="px-6 py-4">
											<div class="space-y-1">
												<CurrencyDisplay
													amount={invoice.amount}
													currency={invoice.currency || 'USD'}
													size="md"
													class="font-medium"
												/>
												<p class="text-muted-foreground text-sm">
													{invoice.currency || 'USD'}
												</p>
											</div>
										</td>
										<td class="px-6 py-4">
											<InvoiceStatusBadge
												status={getInvoiceStatus(invoice)}
											/>
										</td>
										<td class="px-6 py-4">
											{#if invoice.dueDate}
												<div class="space-y-1">
													<p class="text-sm">
														{formatDate(invoice.dueDate, 'short')}
													</p>
													{#if isOverdue(invoice.dueDate) && invoice.status !== 'Paid' && invoice.status !== 'Cancelled'}
														<p class="text-xs text-orange-600">
															Overdue
														</p>
													{/if}
												</div>
											{:else}
												<span class="text-muted-foreground text-sm"
													>No due date</span
												>
											{/if}
										</td>
										<td class="px-6 py-4">
											<p class="text-sm">
												{formatDate(invoice.createdDateUtc, 'short')}
											</p>
										</td>
										<td class="px-6 py-4">
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
			<div class="flex items-center justify-between border-t pt-6">
				<p class="text-muted-foreground text-sm">
					Showing {filteredInvoices.length} of {invoices.length} invoices
				</p>
				<div class="text-muted-foreground flex items-center gap-4 text-sm">
					<span class="flex items-center gap-1">
						<div class="h-2 w-2 rounded-full bg-green-500"></div>
						{summary.paidCount} paid
					</span>
					<span class="flex items-center gap-1">
						<div class="h-2 w-2 rounded-full bg-orange-500"></div>
						{summary.overdueCount} overdue
					</span>
				</div>
			</div>
		{/if}
</div>
