<script lang="ts">
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
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
	import {
		invoiceApi,
		type Invoice,
		type MarkInvoiceAsPaidRequest,
		type SimulatePaymentRequest
	} from '$lib';
	import InvoiceStatusBadge from '$lib/components/InvoiceStatusBadge.svelte';
	import { CurrencyDisplay } from '$lib/components/ui/currency-display';
	import { formatCurrency } from '$lib/utils/currency.js';
	import { formatDate, formatDateForInput } from '$lib/utils/date.js';

	type Props = {
		data: {
			invoice: Invoice;
		};
	};
	
	let { data }: Props = $props();
	let invoice = $state<Invoice>(data.invoice);
	let invoiceId = $derived($page.params.id);
	let error = $state<string | null>(null);
	let actionLoading = $state<string | null>(null);

	// Form states for actions
	let markAsPaidForm = $state({
		amountPaid: 0,
		paymentDate: formatDateForInput()
	});

	let simulatePaymentForm = $state({
		amount: 0,
		currency: 'USD',
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


	async function markAsPaid() {
		if (!invoice || actionLoading) return;

		actionLoading = 'mark-paid';
		try {
			const request: MarkInvoiceAsPaidRequest = {
				amountPaid: markAsPaidForm.amountPaid,
				paymentDate: markAsPaidForm.paymentDate
			};

			const updatedInvoice = await invoiceApi.markInvoiceAsPaid(invoice.invoiceId, request);
			invoice = updatedInvoice;
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to mark invoice as paid';
		} finally {
			actionLoading = null;
		}
	}

	async function simulatePayment() {
		if (!invoice || actionLoading) return;

		actionLoading = 'simulate-payment';
		try {
			const request: SimulatePaymentRequest = {
				amount: simulatePaymentForm.amount,
				currency: simulatePaymentForm.currency,
				paymentMethod: simulatePaymentForm.paymentMethod,
				paymentReference: simulatePaymentForm.paymentReference || undefined
			};

			await invoiceApi.simulatePayment(invoice.invoiceId, request);
			// Refresh the page to get updated invoice status
			window.location.reload();
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to simulate payment';
		} finally {
			actionLoading = null;
		}
	}

	async function cancelInvoice() {
		if (!invoice || actionLoading) return;

		if (!confirm(`Are you sure you want to cancel invoice "${invoice.name}"?`)) {
			return;
		}

		actionLoading = 'cancel';
		try {
			const updatedInvoice = await invoiceApi.cancelInvoice(invoice.invoiceId);
			invoice = updatedInvoice;
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to cancel invoice';
		} finally {
			actionLoading = null;
		}
	}

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

	// Initialize form values with invoice data
	markAsPaidForm.amountPaid = invoice.amount;
	simulatePaymentForm.amount = invoice.amount;
	simulatePaymentForm.currency = invoice.currency || 'USD';
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

	{#if error}
		<Card class="border-destructive/50 bg-destructive/5">
			<CardContent class="flex flex-col items-center justify-center space-y-4 py-12">
				<div class="bg-destructive/10 rounded-full p-3">
					<XCircle size={24} class="text-destructive" />
				</div>
				<div class="space-y-2 text-center">
					<h3 class="text-destructive font-semibold">Error</h3>
					<p class="text-muted-foreground max-w-md text-sm">{error}</p>
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
							<div class="space-y-3">
								<h4 class="flex items-center gap-2 font-medium">
									<CheckCircle size={16} class="text-green-600" />
									Mark as Paid
								</h4>
								<div class="space-y-3">
									<div>
										<label class="text-sm font-medium">Amount Paid</label>
										<CurrencyInput
											bind:value={markAsPaidForm.amountPaid}
											currency={invoice.currency || 'USD'}
											placeholder="Enter payment amount"
										/>
									</div>
									<div>
										<label class="text-sm font-medium">Payment Date</label>
										<Input
											type="date"
											bind:value={markAsPaidForm.paymentDate}
										/>
									</div>
								</div>
								<Button
									onclick={markAsPaid}
									disabled={!!actionLoading || markAsPaidForm.amountPaid <= 0}
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
							</div>
							<div class="border-t pt-4"></div>
						{/if}

						<!-- Simulate Payment -->
						{#if canSimulatePayment}
							<div class="space-y-3">
								<h4 class="flex items-center gap-2 font-medium">
									<CreditCard size={16} class="text-blue-600" />
									Simulate Payment
								</h4>
								<div class="space-y-3">
									<div>
										<label class="text-sm font-medium">Amount</label>
										<CurrencyInput
											bind:value={simulatePaymentForm.amount}
											currency={invoice.currency || 'USD'}
											placeholder="Enter simulation amount"
										/>
									</div>
									<div>
										<label class="text-sm font-medium">Payment Method</label>
										<Select
											bind:value={simulatePaymentForm.paymentMethod}
											options={paymentMethodOptions}
											placeholder="Select payment method"
										/>
									</div>
									<div>
										<label class="text-sm font-medium"
											>Reference (Optional)</label
										>
										<Input
											bind:value={simulatePaymentForm.paymentReference}
											placeholder="Payment reference"
										/>
									</div>
								</div>
								<Button
									onclick={simulatePayment}
									variant="outline"
									disabled={!!actionLoading || simulatePaymentForm.amount <= 0}
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
							</div>
							<div class="border-t pt-4"></div>
						{/if}

						<!-- Cancel Invoice -->
						{#if canCancel}
							<div class="space-y-3">
								<h4 class="flex items-center gap-2 font-medium">
									<XCircle size={16} class="text-red-600" />
									Cancel Invoice
								</h4>
								<p class="text-muted-foreground text-sm">
									This action cannot be undone. The invoice will be marked as
									cancelled.
								</p>
								<Button
									onclick={cancelInvoice}
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
							</div>
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
