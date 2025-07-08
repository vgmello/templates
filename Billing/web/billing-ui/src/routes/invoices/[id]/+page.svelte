<script lang="ts">
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import { enhance } from '$app/forms';
	import { Button } from '$lib/components/ui/button';
	import { Card, CardHeader, CardTitle, CardContent } from '$lib/components/ui/card';
	import { Input } from '$lib/components/ui/input';
	import { CurrencyInput } from '$lib/components/ui/currency-input';
	import { Select } from '$lib/components/ui/select';
	import {
		ArrowLeft,
		Edit,
		CheckCircle,
		CreditCard,
		XCircle,
		Calendar,
		DollarSign,
		Hash,
		User,
		Clock,
		Copy
	} from '@lucide/svelte';
	import { type Invoice } from '$lib/api';
	import InvoiceStatusBadge from '$lib/components/InvoiceStatusBadge.svelte';
	import { CurrencyDisplay } from '$lib/components/ui/currency-display';
	import { formatCurrency } from '$lib/utils/currency.js';
	import { formatDate, formatDateForInput } from '$lib/utils/date.js';

	type Props = {
		data: {
			invoice: Invoice;
		};
		form?: {
			success?: boolean;
			errorMessage?: string;
		};
	};
	
	let { data, form }: Props = $props();
	let invoice = $state<Invoice>(data.invoice);
	let invoiceId = $derived($page.params.id);
	let actionLoading = $state<string | null>(null);

	// Form states for actions
	let markAsPaidForm = $state({
		amountPaid: invoice.amount,
		paymentDate: formatDateForInput()
	});

	let simulatePaymentForm = $state({
		amount: invoice.amount,
		currency: invoice.currency || 'USD',
		paymentMethod: 'Credit Card',
		paymentReference: ''
	});

	// Payment method options
	const paymentMethodOptions = [
		{ value: 'Credit Card', label: 'Credit Card' },
		{ value: 'Bank Transfer', label: 'Bank Transfer' },
		{ value: 'PayPal', label: 'PayPal' },
		{ value: 'Cash', label: 'Cash' }
	];

	// Form enhancement handlers
	const handleMarkAsPaid = () => {
		actionLoading = 'mark-paid';
		return async ({ result }) => {
			actionLoading = null;
			if (result.type === 'success') {
				// Refresh the page to get updated invoice
				window.location.reload();
			}
		};
	};

	const handleSimulatePayment = () => {
		actionLoading = 'simulate-payment';
		return async ({ result }) => {
			actionLoading = null;
			if (result.type === 'success') {
				// Refresh the page to get updated invoice
				window.location.reload();
			}
		};
	};

	const handleCancel = () => {
		if (!confirm(`Are you sure you want to cancel invoice "${invoice.name}"?`)) {
			return false;
		}
		actionLoading = 'cancel';
		return async ({ result }) => {
			actionLoading = null;
			if (result.type === 'success') {
				// Refresh the page to get updated invoice
				window.location.reload();
			}
		};
	};

	function goBack() {
		goto('/invoices');
	}

	function editInvoice() {
		if (invoice) {
			goto(`/invoices/${invoice.invoiceId}/edit`);
		}
	}

	async function copyInvoiceId(invoiceId: string | null) {
		if (invoiceId == null) {
			return;
		}

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

	// Check if actions are available based on status
	let canMarkAsPaid = $derived(invoice?.status === 'Draft' || invoice?.status === 'Pending');
	let canSimulatePayment = $derived(invoice?.status === 'Draft' || invoice?.status === 'Pending');
	let canCancel = $derived(invoice?.status === 'Draft' || invoice?.status === 'Pending');
</script>

<svelte:head>
	<title>{invoice?.name || 'Invoice'} - Billing System</title>
</svelte:head>

<div class="container mx-auto space-y-8 p-6">
	<!-- Header with back navigation -->
	<div class="flex items-center gap-4">
		<Button variant="ghost" onclick={goBack} class="gap-2">
			<ArrowLeft size={16} />
			Back to Invoices
		</Button>
		<div class="bg-border h-6 w-px"></div>
		<Button variant="outline" size="sm" onclick={editInvoice} class="gap-2">
			<Edit size={14} />
			Edit
		</Button>
	</div>

	{#if form?.errorMessage}
		<Card class="border-destructive/50 bg-destructive/5">
			<CardContent class="flex flex-col items-center justify-center space-y-4 py-12">
				<div class="bg-destructive/10 rounded-full p-3">
					<XCircle size={24} class="text-destructive" />
				</div>
				<div class="space-y-2 text-center">
					<h3 class="text-destructive font-semibold">Error</h3>
					<p class="text-muted-foreground max-w-md text-sm">{form.errorMessage}</p>
				</div>
			</CardContent>
		</Card>
	{/if}
	
	{#if invoice}
		<div class="grid grid-cols-1 gap-8 lg:grid-cols-3">
			<!-- Invoice Information -->
			<div class="space-y-6 lg:col-span-2">
				<!-- Invoice Header -->
				<div class="space-y-4">
					<div class="flex items-start justify-between">
						<div class="space-y-2">
							<h1 class="text-3xl font-bold tracking-tight">{invoice.name}</h1>
							<div class="flex items-center gap-2">
								<InvoiceStatusBadge status={invoice.status} />
								<button
									onclick={() => copyInvoiceId(invoice?.invoiceId ?? null)}
									class="text-muted-foreground hover:text-foreground group flex items-center gap-1 font-mono text-sm transition-colors"
								>
									<span class="text-muted-foreground text-sm">
										Invoice ID: {invoice.invoiceId}
									</span>
									<Copy size={12} />
								</button>
							</div>
						</div>
					</div>
				</div>

				<!-- Invoice Details Card -->
				<Card>
					<CardHeader>
						<CardTitle>Invoice Information</CardTitle>
					</CardHeader>
					<CardContent class="space-y-6">
						<div class="grid grid-cols-1 gap-6 md:grid-cols-2">
							<!-- Amount -->
							<div class="space-y-2">
								<label
									class="text-muted-foreground flex items-center gap-2 text-sm font-medium"
								>
									<DollarSign size={14} />
									Amount
								</label>
								<CurrencyDisplay
									amount={invoice.amount}
									currency={invoice.currency || 'USD'}
									size="xl"
									variant="accent"
								/>
							</div>

							<!-- Currency -->
							<div class="space-y-2">
								<label class="text-muted-foreground text-sm font-medium"
									>Currency</label
								>
								<p class="text-lg">{invoice.currency || 'USD'}</p>
							</div>

							<!-- Status -->
							<div class="space-y-2">
								<label class="text-muted-foreground text-sm font-medium"
									>Status</label
								>
								<InvoiceStatusBadge status={invoice.status} />
							</div>

							<!-- Due Date -->
							<div class="space-y-2">
								<label
									class="text-muted-foreground flex items-center gap-2 text-sm font-medium"
								>
									<Calendar size={14} />
									Due Date
								</label>
								<p class="text-lg">
									{invoice.dueDate
										? formatDate(invoice.dueDate, 'medium')
										: 'No due date set'}
								</p>
							</div>

							<!-- Created -->
							<div class="space-y-2">
								<label
									class="text-muted-foreground flex items-center gap-2 text-sm font-medium"
								>
									<Clock size={14} />
									Created
								</label>
								<p class="text-sm">{formatDate(invoice.createdDateUtc, 'long')}</p>
							</div>

							<!-- Updated -->
							<div class="space-y-2">
								<label
									class="text-muted-foreground flex items-center gap-2 text-sm font-medium"
								>
									<Clock size={14} />
									Updated
								</label>
								<p class="text-sm">{formatDate(invoice.updatedDateUtc, 'long')}</p>
							</div>
						</div>
					</CardContent>
				</Card>
			</div>

			<!-- Actions Panel -->
			<div class="space-y-6">
				<Card>
					<CardHeader>
						<CardTitle>Actions</CardTitle>
						<p class="text-muted-foreground text-sm">Manage this invoice</p>
					</CardHeader>
					<CardContent class="space-y-4">
						<!-- Mark as Paid -->
						{#if canMarkAsPaid}
							<form
								method="POST"
								action="?/markPaid"
								use:enhance={handleMarkAsPaid}
								class="space-y-3"
							>
								<h4 class="flex items-center gap-2 font-medium">
									<CheckCircle size={16} class="text-green-600" />
									Mark as Paid
								</h4>
								<div class="space-y-3">
									<div>
										<label class="text-sm font-medium">Amount Paid</label>
										<input
											type="number"
											name="amountPaid"
											step="0.01"
											min="0"
											value={markAsPaidForm.amountPaid}
											class="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm transition-colors file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
											placeholder="Enter payment amount"
											required
										/>
									</div>
									<div>
										<label class="text-sm font-medium">Payment Date</label>
										<Input
											type="date"
											name="paymentDate"
											value={markAsPaidForm.paymentDate}
										/>
									</div>
								</div>
								<Button
									type="submit"
									disabled={!!actionLoading}
									class="w-full gap-2 bg-green-600 hover:bg-green-700"
								>
									{#if actionLoading === 'mark-paid'}
										<div
											class="h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent"
										></div>
									{:else}
										<CheckCircle size={16} />
									{/if}
									Mark as Paid
								</Button>
							</form>
							<div class="border-t pt-4"></div>
						{/if}

						<!-- Simulate Payment -->
						{#if canSimulatePayment}
							<form
								method="POST"
								action="?/simulatePayment"
								use:enhance={handleSimulatePayment}
								class="space-y-3"
							>
								<h4 class="flex items-center gap-2 font-medium">
									<CreditCard size={16} class="text-blue-600" />
									Simulate Payment
								</h4>
								<div class="space-y-3">
									<div>
										<label class="text-sm font-medium">Amount</label>
										<input
											type="number"
											name="amount"
											step="0.01"
											min="0"
											value={simulatePaymentForm.amount}
											class="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm transition-colors file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
											placeholder="Enter simulation amount"
											required
										/>
									</div>
									<div>
										<label class="text-sm font-medium">Currency</label>
										<Input
											type="text"
											name="currency"
											value={simulatePaymentForm.currency}
											placeholder="Currency"
										/>
									</div>
									<div>
										<label class="text-sm font-medium">Payment Method</label>
										<select
											name="paymentMethod"
											value={simulatePaymentForm.paymentMethod}
											class="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm transition-colors file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
										>
											{#each paymentMethodOptions as option}
												<option value={option.value}>{option.label}</option>
											{/each}
										</select>
									</div>
									<div>
										<label class="text-sm font-medium"
											>Reference (Optional)</label
										>
										<Input
											type="text"
											name="paymentReference"
											value={simulatePaymentForm.paymentReference}
											placeholder="Payment reference"
										/>
									</div>
								</div>
								<Button
									type="submit"
									variant="outline"
									disabled={!!actionLoading}
									class="w-full gap-2"
								>
									{#if actionLoading === 'simulate-payment'}
										<div
											class="h-4 w-4 animate-spin rounded-full border-2 border-current border-t-transparent"
										></div>
									{:else}
										<CreditCard size={16} />
									{/if}
									Simulate Payment
								</Button>
							</form>
							<div class="border-t pt-4"></div>
						{/if}

						<!-- Cancel Invoice -->
						{#if canCancel}
							<form
								method="POST"
								action="?/cancel"
								use:enhance={handleCancel}
								class="space-y-3"
							>
								<h4 class="flex items-center gap-2 font-medium">
									<XCircle size={16} class="text-red-600" />
									Cancel Invoice
								</h4>
								<p class="text-muted-foreground text-sm">
									This action cannot be undone. The invoice will be marked as
									cancelled.
								</p>
								<Button
									type="submit"
									variant="destructive"
									disabled={!!actionLoading}
									class="w-full gap-2"
								>
									{#if actionLoading === 'cancel'}
										<div
											class="h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent"
										></div>
									{:else}
										<XCircle size={16} />
									{/if}
									Cancel Invoice
								</Button>
							</form>
						{/if}

						<!-- No actions available -->
						{#if !canMarkAsPaid && !canSimulatePayment && !canCancel}
							<div class="space-y-2 py-8 text-center">
								<p class="text-muted-foreground text-sm">No actions available</p>
								<p class="text-muted-foreground text-xs">
									This invoice is {invoice.status.toLowerCase()} and cannot be modified.
								</p>
							</div>
						{/if}
					</CardContent>
				</Card>
			</div>
		</div>
	{/if}
</div>