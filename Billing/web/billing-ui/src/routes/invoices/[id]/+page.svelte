<script lang="ts">
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import { Button } from '$lib/components/ui/button';
	import { Card, CardHeader, CardTitle, CardContent } from '$lib/components/ui/card';
	import { Input } from '$lib/components/ui/input';
	import { 
		ArrowLeft, Edit, CheckCircle, CreditCard, XCircle, 
		Calendar, DollarSign, Hash, User, Clock 
	} from '@lucide/svelte';
	import { invoiceApi, type Invoice, type MarkInvoiceAsPaidRequest, type SimulatePaymentRequest } from '$lib';
	import InvoiceStatusBadge from '$lib/components/InvoiceStatusBadge.svelte';
	import { formatCurrency } from '$lib/utils/currency.js';
	import { formatDate, formatDateForInput } from '$lib/utils/date.js';

	let invoiceId = $derived($page.params.id);
	let invoice = $state<Invoice | null>(null);
	let loading = $state(false);
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

	async function loadInvoice() {
		if (!invoiceId) return;
		
		loading = true;
		error = null;
		
		try {
			invoice = await invoiceApi.getInvoice(invoiceId);
			// Initialize form values
			if (invoice) {
				markAsPaidForm.amountPaid = invoice.amount;
				simulatePaymentForm.amount = invoice.amount;
				simulatePaymentForm.currency = invoice.currency || 'USD';
			}
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to load invoice';
			console.error('Error loading invoice:', err);
		} finally {
			loading = false;
		}
	}

	async function markAsPaid() {
		if (!invoice || actionLoading) return;
		
		actionLoading = 'mark-paid';
		try {
			const request: MarkInvoiceAsPaidRequest = {
				amountPaid: markAsPaidForm.amountPaid,
				paymentDate: markAsPaidForm.paymentDate
			};
			
			invoice = await invoiceApi.markInvoiceAsPaid(invoice.invoiceId, request);
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
			// Reload invoice to get updated status
			await loadInvoice();
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
			invoice = await invoiceApi.cancelInvoice(invoice.invoiceId);
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to cancel invoice';
		} finally {
			actionLoading = null;
		}
	}

	function goBack() {
		goto('/invoices');
	}

	// Check if actions are available based on status
	let canMarkAsPaid = $derived(invoice?.status === 'Draft' || invoice?.status === 'Pending');
	let canSimulatePayment = $derived(invoice?.status === 'Draft' || invoice?.status === 'Pending');
	let canCancel = $derived(invoice?.status === 'Draft' || invoice?.status === 'Pending');

	onMount(() => {
		loadInvoice();
	});
</script>

<svelte:head>
	<title>{invoice?.name || 'Invoice'} - Billing System</title>
</svelte:head>

<div class="container mx-auto p-6 space-y-8">
	<!-- Header with back navigation -->
	<div class="flex items-center gap-4">
		<Button variant="ghost" onclick={goBack} class="gap-2">
			<ArrowLeft size={16} />
			Back to Invoices
		</Button>
		<div class="h-6 w-px bg-border"></div>
		<Button variant="outline" size="sm" class="gap-2">
			<Edit size={14} />
			Edit
		</Button>
	</div>

	{#if loading}
		<div class="flex flex-col items-center justify-center py-24 space-y-4">
			<div class="animate-spin rounded-full h-12 w-12 border-2 border-primary border-t-transparent"></div>
			<div class="text-center space-y-1">
				<p class="font-medium">Loading invoice</p>
				<p class="text-sm text-muted-foreground">Please wait while we fetch the invoice details...</p>
			</div>
		</div>
	{:else if error}
		<Card class="border-destructive/50 bg-destructive/5">
			<CardContent class="flex flex-col items-center justify-center py-12 space-y-4">
				<div class="rounded-full bg-destructive/10 p-3">
					<XCircle size={24} class="text-destructive" />
				</div>
				<div class="text-center space-y-2">
					<h3 class="font-semibold text-destructive">Error Loading Invoice</h3>
					<p class="text-sm text-muted-foreground max-w-md">{error}</p>
				</div>
				<Button onclick={loadInvoice} variant="outline" class="gap-2">
					<ArrowLeft size={16} />
					Try Again
				</Button>
			</CardContent>
		</Card>
	{:else if invoice}
		<div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
			<!-- Invoice Information -->
			<div class="lg:col-span-2 space-y-6">
				<!-- Invoice Header -->
				<div class="space-y-4">
					<div class="flex items-start justify-between">
						<div class="space-y-2">
							<h1 class="text-3xl font-bold tracking-tight">{invoice.name}</h1>
							<div class="flex items-center gap-2">
								<InvoiceStatusBadge status={invoice.status} />
								<span class="text-sm text-muted-foreground">
									Invoice ID: {invoice.invoiceId.slice(0, 8)}...
								</span>
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
						<div class="grid grid-cols-1 md:grid-cols-2 gap-6">
							<!-- Amount -->
							<div class="space-y-2">
								<label class="text-sm font-medium text-muted-foreground flex items-center gap-2">
									<DollarSign size={14} />
									Amount
								</label>
								<p class="text-2xl font-bold">{formatCurrency(invoice.amount, invoice.currency)}</p>
							</div>

							<!-- Currency -->
							<div class="space-y-2">
								<label class="text-sm font-medium text-muted-foreground">Currency</label>
								<p class="text-lg">{invoice.currency || 'USD'}</p>
							</div>

							<!-- Status -->
							<div class="space-y-2">
								<label class="text-sm font-medium text-muted-foreground">Status</label>
								<InvoiceStatusBadge status={invoice.status} />
							</div>

							<!-- Due Date -->
							<div class="space-y-2">
								<label class="text-sm font-medium text-muted-foreground flex items-center gap-2">
									<Calendar size={14} />
									Due Date
								</label>
								<p class="text-lg">
									{invoice.dueDate ? formatDate(invoice.dueDate, 'medium') : 'No due date set'}
								</p>
							</div>

							<!-- Created -->
							<div class="space-y-2">
								<label class="text-sm font-medium text-muted-foreground flex items-center gap-2">
									<Clock size={14} />
									Created
								</label>
								<p class="text-sm">{formatDate(invoice.createdDateUtc, 'long')}</p>
							</div>

							<!-- Updated -->
							<div class="space-y-2">
								<label class="text-sm font-medium text-muted-foreground flex items-center gap-2">
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
						<p class="text-sm text-muted-foreground">Manage this invoice</p>
					</CardHeader>
					<CardContent class="space-y-4">
						<!-- Mark as Paid -->
						{#if canMarkAsPaid}
							<div class="space-y-3">
								<h4 class="font-medium flex items-center gap-2">
									<CheckCircle size={16} class="text-green-600" />
									Mark as Paid
								</h4>
								<div class="space-y-3">
									<div>
										<label class="text-sm font-medium">Amount Paid</label>
										<Input
											type="number"
											step="0.01"
											bind:value={markAsPaidForm.amountPaid}
											placeholder="0.00"
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
										<div class="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></div>
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
								<h4 class="font-medium flex items-center gap-2">
									<CreditCard size={16} class="text-blue-600" />
									Simulate Payment
								</h4>
								<div class="space-y-3">
									<div>
										<label class="text-sm font-medium">Amount</label>
										<Input
											type="number"
											step="0.01"
											bind:value={simulatePaymentForm.amount}
											placeholder="0.00"
										/>
									</div>
									<div>
										<label class="text-sm font-medium">Payment Method</label>
										<select 
											bind:value={simulatePaymentForm.paymentMethod}
											class="w-full px-3 py-2 border border-input bg-background text-sm rounded-md focus:outline-none focus:ring-2 focus:ring-ring"
										>
											<option value="Credit Card">Credit Card</option>
											<option value="Bank Transfer">Bank Transfer</option>
											<option value="PayPal">PayPal</option>
											<option value="Cash">Cash</option>
										</select>
									</div>
									<div>
										<label class="text-sm font-medium">Reference (Optional)</label>
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
										<div class="animate-spin rounded-full h-4 w-4 border-2 border-current border-t-transparent"></div>
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
								<h4 class="font-medium flex items-center gap-2">
									<XCircle size={16} class="text-red-600" />
									Cancel Invoice
								</h4>
								<p class="text-sm text-muted-foreground">
									This action cannot be undone. The invoice will be marked as cancelled.
								</p>
								<Button 
									onclick={cancelInvoice} 
									variant="destructive"
									disabled={!!actionLoading}
									class="w-full gap-2"
								>
									{#if actionLoading === 'cancel'}
										<div class="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></div>
									{:else}
										<XCircle size={16} />
									{/if}
									Cancel Invoice
								</Button>
							</div>
						{/if}

						<!-- No actions available -->
						{#if !canMarkAsPaid && !canSimulatePayment && !canCancel}
							<div class="text-center py-8 space-y-2">
								<p class="text-sm text-muted-foreground">No actions available</p>
								<p class="text-xs text-muted-foreground">
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